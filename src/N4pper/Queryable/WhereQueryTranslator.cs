using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class WhereQueryTranslator : QueryPartTranslatorBase
    {
        protected override bool MatchCall(MethodCallExpression expression)
        {
            return expression.Method.Name == "Where";
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == "Where")
                {
                    _builder.Append(" WITH * WHERE (");

                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    Visit(lambda.Body);

                    _builder.Append(") ");
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }
            else if (m.Method.DeclaringType == typeof(String))
            {
                if (m.Method.Name == "StartsWith")
                {
                    Visit(m.Object);

                    _builder.Append(" STARTS WITH ");

                    Visit(m.Arguments);
                }
                else if (m.Method.Name == "EndsWith")
                {
                    Visit(m.Object);

                    _builder.Append(" ENDS WITH ");

                    Visit(m.Arguments);
                }
                else if (m.Method.Name == "Contains")
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
