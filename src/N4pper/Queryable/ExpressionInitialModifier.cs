using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class ExpressionInitialModifier : ExpressionVisitor
    {
        private IQueryable _queryableRecords;
        private int _currentCount = 0;
        private int _countFromBegin;

        internal ExpressionInitialModifier(IQueryable records, int countFromBegin)
        {
            _countFromBegin = countFromBegin;
            _queryableRecords = records ?? throw new ArgumentNullException(nameof(records));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _currentCount++;

            if (_currentCount == _countFromBegin)
            {
                return VisitFinalMethodCall(node as MethodCallExpression);
            }
            else
                return Visit(node);
        }
        protected Expression VisitFinalMethodCall(MethodCallExpression node)
        {
            List<Expression> param = node.Arguments.ToList();
            param[0] = Expression.Constant(_queryableRecords);
            return Expression.Call(null, node.Method,param.ToArray());
        }
    }
}
