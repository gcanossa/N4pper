using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class OrderByQueryTranslator : QueryPartTranslatorBase
    {
        internal OrderByQueryTranslator()
        {
        }

        protected override bool MatchCall(MethodCallExpression expression)
        {
            string[] names = new[] { "OrderBy", "ThenBy", "OrderByDescending", "ThenByDescending" };
            return names.Contains(expression.Method.Name);
        }
        
        private Expression ManageOrderBy(MethodCallExpression m)
        {
            LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            
            Visit(lambda.Body);

            if (m.Method.Name.EndsWith("Descending"))
                _builder.Append(" DESC");
            else
                _builder.Append(" ASC");

            if (m != _callChain.Last())
                _builder.Append(",");

            return m;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == "OrderBy" || m.Method.Name == "OrderByDescending")
                {
                    if (string.IsNullOrEmpty(_builder.ToString()))
                        _builder.Append(" ORDER BY ");

                    return ManageOrderBy(m);
                }
                else if (m.Method.Name == "ThenBy" || m.Method.Name == "ThenByDescending")
                {
                    return ManageOrderBy(m);
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
