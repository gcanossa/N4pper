using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable.Translators
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
                ParameterExpression par = lambda.Parameters[0];
                MemberExpression tmp = lambda.Body as MemberExpression;

                if (par.Type.GetProperty(tmp.Member.Name) != null)
                {
                    Visit(tmp);

                    return true;
                }
            }
            else if (lambda.Body.NodeType == ExpressionType.New)
            {
                ParameterExpression par = lambda.Parameters[0];
                NewExpression tmp = lambda.Body as NewExpression;

                if (tmp.Type.GetProperties().All(p => par.Type.GetProperty(p.Name) != null))
                {
                    base.Visit(tmp);

                    return true;
                }
            }

            return false;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            for (int i=0;i<node.Arguments.Count; i++)
            {
                if (node.Arguments[i].NodeType == ExpressionType.Constant)
                {
                    _builder.Append(node.Members[i].Name);
                    _builder.Append(":");
                }
                Visit(node.Arguments[i]);
                _builder.Append(",");
            }
            _builder.Remove(_builder.Length - 1, 1);

            return node;
        }
        protected override Expression VisitMember(MemberExpression m)
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
                if (m.Method.Name == nameof(q.Select) && m.Arguments[1].Type.GetGenericArguments()[0].GetGenericArguments().Length==2)
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" WITH {");

                    if (!HandleVisitSelectBody(lambda))
                        throw new ArgumentOutOfRangeException(nameof(m), $"The Select method can only be used to narrow the type");

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
