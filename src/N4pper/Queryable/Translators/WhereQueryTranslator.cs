using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using q = System.Linq.Queryable;

namespace N4pper.Queryable.Translators
{
    internal class WhereQueryTranslator : QueryPartTranslatorBase
    {
        internal WhereQueryTranslator(Type typeResult) : base(typeResult)
        {
        }
        
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == nameof(q.Where) && m.Arguments[1].Type.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                {
                    _builder.Append(" WITH * WHERE (");

                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    Visit(lambda.Body);

                    _builder.Append(") ");
                }
                else if (m.Method.Name == nameof(q.Distinct) && m.Arguments.Count==1)
                {
                    _builder.Append(" WITH DISTINCT *");
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }
            else if (m.Method.DeclaringType == typeof(Enumerable))
            {
                if (m.Method.Name == nameof(Enumerable.Contains) && m.Arguments.Count == 2)
                {
                    Visit(m.Arguments[1]);

                    _builder.Append(" IN ");

                    Visit(m.Arguments[0]);
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }
            else if (m.Method.DeclaringType == typeof(Regex))
            {
                if (m.Method.Name == nameof(Regex.IsMatch))
                {
                    Visit(m.Arguments[0]);

                    _builder.Append(" =~ ");

                    Visit(m.Arguments[1]);
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
