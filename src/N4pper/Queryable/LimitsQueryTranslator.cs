using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class LimitsQueryTranslator : QueryPartTranslatorBase
    {
        internal LimitsQueryTranslator()
        {
        }

        protected override void ValidateChain()
        {
            if (_callChain.Count(p => p.Method.Name == "Skip") > 1)
                throw new ArgumentOutOfRangeException("expression", "Only one call to 'Skip' is allowed");
            if (_callChain.Count(p => p.Method.Name == "Take") > 1)
                throw new ArgumentOutOfRangeException("expression", "Only one call to 'Take' is allowed");
        }
        protected override void SortChain()
        {
            _callChain.Sort((a, b) => a.Method.Name == "Skip" ? -1 : 1);
        }

        protected override bool MatchCall(MethodCallExpression expression)
        {
            string[] names = new[] { "Skip", "Take" };
            return names.Contains(expression.Method.Name);
        }

        private Expression ManageOrderBy(MethodCallExpression m)
        {
            LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

            _builder.Append(" ");

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
                if (m.Method.Name == "Skip")
                {
                    _builder.Append(" SKIP ");
                    Visit(m.Arguments[1]);
                }
                else if (m.Method.Name == "Take")
                {
                    _builder.Append(" LIMIT ");
                    Visit(m.Arguments[1]);
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
