using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable.Translators
{
    internal class OrderByQueryTranslator : QueryPartTranslatorBase
    {
        internal OrderByQueryTranslator(Type typeResult) : base(typeResult)
        {
        }
        
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            string[] names = new[] { nameof(q.OrderBy), nameof(q.ThenBy), nameof(q.OrderByDescending), nameof(q.ThenByDescending) };

            if (m.Method.DeclaringType == typeof(System.Linq.Queryable) && names.Contains(m.Method.Name))
            {
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                if(_builder.Length>0)
                    _builder.Append(",");

                Visit(lambda.Body);

                if (m.Method.Name.EndsWith("Descending"))
                    _builder.Append(" DESC");
                else
                    _builder.Append(" ASC");

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
