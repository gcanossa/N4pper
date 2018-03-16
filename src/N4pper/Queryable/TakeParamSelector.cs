using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable
{
    internal class TakeParamSelector : ExpressionVisitor
    {
        public int? TakeParam { get; set; }

        internal void Accept(Expression expression)
        {
            Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable) && m.Method.Name == nameof(q.Take))
            {
                int val = (int)((ConstantExpression)m.Arguments[1]).Value;
                TakeParam = val < (TakeParam??int.MaxValue) ? val : TakeParam;

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
