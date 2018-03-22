using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable.Translators
{
    internal class SkipParamSelector : ExpressionVisitor
    {
        public int SkipParam { get; set; } = 0;

        internal void Accept(Expression expression)
        {
            Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable) && m.Method.Name == nameof(q.Skip))
            {
                int val = (int)((ConstantExpression)m.Arguments[1]).Value;
                SkipParam += val;

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
