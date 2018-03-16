using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class ExpressionInitialCollectionModifier : ExpressionVisitor
    {
        private IQueryable _queryableRecords;
        private MethodCallExpression _initialCall;
        private bool _isInitialCall = false;

        internal ExpressionInitialCollectionModifier(IQueryable records, MethodCallExpression initialCall)
        {
            _initialCall = initialCall ?? throw new ArgumentNullException(nameof(initialCall));
            _queryableRecords = records ?? throw new ArgumentNullException(nameof(records));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _isInitialCall = node == _initialCall;
            Expression result = base.VisitMethodCall(node);
            _isInitialCall = false;

            return result;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (_isInitialCall && c.Type.Name == "QueryableNeo4jStatement`1")
                return Expression.Constant(_queryableRecords);
            else
                return c;
        }
    }
}
