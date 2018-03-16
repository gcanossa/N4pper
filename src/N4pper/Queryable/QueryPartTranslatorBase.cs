using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal abstract class QueryPartTranslatorBase : ExpressionVisitor
    {
        protected StringBuilder _builder = new StringBuilder();
        public Type TypeResult { get; set; }

        public string Statement => _builder.ToString();

        internal QueryPartTranslatorBase(Type typeResult)
        {
            TypeResult = typeResult;
        }
        
        internal void Accept(Expression expression)
        {
            Visit(expression);
        }

        protected Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            
            return e;
        }
        
        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    _builder.Append(" NOT ");

                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }
            
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _builder.Append("(");

            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _builder.Append(" AND ");

                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _builder.Append(" OR");

                    break;
                case ExpressionType.Equal:
                    if (b.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)b.Right).Value == null)
                        _builder.Append(" IS ");
                    else
                        _builder.Append(" = ");

                    break;
                case ExpressionType.NotEqual:
                    if (b.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)b.Right).Value == null)
                        _builder.Append(" IS NOT ");
                    else
                        _builder.Append(" <> ");

                    break;
                case ExpressionType.LessThan:
                    _builder.Append(" < ");

                    break;
                case ExpressionType.LessThanOrEqual:
                    _builder.Append(" <= ");

                    break;
                case ExpressionType.GreaterThan:
                    _builder.Append(" > ");

                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _builder.Append(" >= ");
                    
                    break;
                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            Visit(b.Right);

            _builder.Append(")");

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
            {
                _builder.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _builder.Append(((bool)c.Value));

                        break;
                    case TypeCode.String:
                        _builder.Append("'");

                        _builder.Append(c.Value);

                        _builder.Append("'");

                        break;
                    case TypeCode.DateTime:
                        DateTimeOffset d = (DateTime)c.Value;
                        _builder.Append(d.ToUnixTimeMilliseconds());

                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException($"Only primitive types are supported. Expression '{c.Value}'");
                    default:
                        _builder.Append(c.Value);

                        break;
                }
            }

            return c;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            foreach (Expression item in node.Arguments)
            {
                Visit(item);
                _builder.Append(",");
            }
            _builder.Remove(_builder.Length - 1, 1);

            return node;
        }
        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                Visit(m.Expression);
                _builder.Append(".");
                _builder.Append(m.Member.Name);
                
                return m;
            }

            throw new NotSupportedException($"Only first level members are allowed in queries. Expression {m}");
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            _builder.Append(node.Name);

            return node;
        }
    }
}
