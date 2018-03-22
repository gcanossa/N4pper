using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable.Translators
{
    internal class FirstQueryTranslator : QueryPartTranslatorBase
    {
        internal FirstQueryTranslator(Type typeResult) : base(typeResult)
        {
        }


        private bool HandleVisitSelectBody(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression tmp = lambda.Body as MemberExpression;

                if (TypeResult.GetProperty(tmp.Member.Name) != null)
                {
                    TypeResult = TypeResult.GetProperty(tmp.Member.Name).PropertyType;

                    Visit(lambda.Body);

                    return true;
                }
            }
            else if (lambda.Body.NodeType == ExpressionType.New)
            {
                NewExpression tmp = lambda.Body as NewExpression;

                if (tmp.Type.GetProperties().All(p => TypeResult.GetProperty(p.Name) != null))
                {
                    TypeResult = tmp.Type;

                    Visit(lambda.Body);

                    return true;
                }
            }

            return false;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == nameof(q.First))
                {
                    _builder.Append(" WITH *");

                    if (m.Arguments.Count == 2)
                    {
                        _builder.Append(" WHERE (");
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                        Visit(lambda.Body);
                        _builder.Append(") ");
                    }
                }
                else if (m.Method.Name == nameof(q.FirstOrDefault))
                {
                    _builder.Append(" WITH *");

                    if (m.Arguments.Count == 2)
                    {
                        _builder.Append(" WHERE (");
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                        Visit(lambda.Body);
                        _builder.Append(") ");
                    }
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }
            else if (m.Method.DeclaringType == typeof(String))
            {
                if (m.Method.Name == nameof(string.StartsWith))
                {
                    Visit(m.Object);

                    _builder.Append(" STARTS WITH ");

                    Visit(m.Arguments);
                }
                else if (m.Method.Name == nameof(string.EndsWith))
                {
                    Visit(m.Object);

                    _builder.Append(" ENDS WITH ");

                    Visit(m.Arguments);
                }
                else if (m.Method.Name == nameof(string.Contains))
                {
                    Visit(m.Object);

                    _builder.Append(" CONTAINS ");

                    Visit(m.Arguments);
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
