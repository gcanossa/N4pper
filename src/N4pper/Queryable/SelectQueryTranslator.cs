using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable
{
    internal class SelectQueryTranslator : QueryPartTranslatorBase
    {
        public IEnumerable<string> OtherVars { get; set; }
        internal SelectQueryTranslator(Type typeResult, IEnumerable<string> otherVariables) : base(typeResult)
        {
            OtherVars = otherVariables ?? new string[0];
        }


        private bool HandleVisitSelectBody(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression tmp = lambda.Body as MemberExpression;

                if (TypeResult.GetProperty(tmp.Member.Name) != null)
                {
                    TypeResult = TypeResult.GetProperty(tmp.Member.Name).PropertyType;

                    VisitMemberForSelect(tmp);

                    return true;
                }
            }
            else if (lambda.Body.NodeType == ExpressionType.New)
            {
                NewExpression tmp = lambda.Body as NewExpression;

                if (tmp.Type.GetProperties().All(p => TypeResult.GetProperty(p.Name) != null))
                {
                    TypeResult = tmp.Type;

                    VisitNewForSelect(tmp);

                    return true;
                }
            }

            return false;
        }
        
        protected Expression VisitNewForSelect(NewExpression node)
        {
            foreach (Expression item in node.Arguments)
            {
                VisitMemberForSelect(item as MemberExpression);
                _builder.Append(",");
            }
            _builder.Remove(_builder.Length - 1, 1);

            return node;
        }
        protected Expression VisitMemberForSelect(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _builder.Append(m.Member.Name);
                _builder.Append(":");
                Visit(m.Expression);
                _builder.Append(".");
                _builder.Append(m.Member.Name);

                return m;
            }

            throw new NotSupportedException($"Only first level members are allowed in queries. Expression {m}");
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == nameof(q.Select) && m.Arguments[1].Type.GetGenericArguments().Length==1)
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" WITH {");

                    if (!HandleVisitSelectBody(lambda))
                        throw new ArgumentOutOfRangeException(nameof(m), $"The Select method can only be used to narrow the type '{TypeResult.FullName}'");

                    _builder.Append("} AS ");
                    Visit(lambda.Parameters[0]);
                    if (OtherVars.Count() > 0)
                        foreach (string item in OtherVars)
                        {
                            _builder.Append($",{item}");
                        }
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
