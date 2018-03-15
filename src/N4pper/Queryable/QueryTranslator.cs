using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class QueryTranslator
    {
        internal QueryTranslator()
        {
        }

        internal void GetExpressionsParams(Expression expression, out List<MethodCallExpression> callChain, out Type typeResult)
        {
            callChain = new List<MethodCallExpression>();
            typeResult = TypeSystem.GetElementType(expression.Type);

            while (expression is MethodCallExpression &&
                ((MethodCallExpression)expression).Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                typeResult = TypeSystem.GetElementType(expression.Type);
                callChain.Add((MethodCallExpression)expression);
                expression = ((MethodCallExpression)expression).Arguments[0];
            }

            callChain.Reverse();
        }

        internal string Translate(Expression expression)
        {
            string[] aggregatedFn = new[] { "Count", "Average", "Sum", "Min", "Max" };
            string[] singleSelector = new[] { "First", "FirstOrDefault" };
            string[] limits = new[] { "Skip", "Take" };
            GetExpressionsParams(expression, out List<MethodCallExpression> callChain, out Type typeResult);

            StringBuilder sb = new StringBuilder();

            sb.Append(new WhereQueryTranslator().Translate(callChain, typeResult));
            if (!aggregatedFn.Contains(callChain.Last().Method.Name))
            {
                sb.Append(new SelectQueryTranslator().Translate(callChain, typeResult));
                sb.Append(new OrderByQueryTranslator().Translate(callChain, typeResult));
                sb.Append(new LimitsQueryTranslator().Translate(callChain, typeResult));
            }
            else
            {
                MethodCallExpression distinct = callChain.FirstOrDefault(p => p.Method.Name == "Distinct");
                if (distinct != null)
                    callChain.Remove(distinct);
                sb.Append(new AggregatedQueryTranslator(distinct != null).Translate(new List<MethodCallExpression>() { callChain.Last() }, null));
                callChain.Remove(callChain.Last());
            }

            if (callChain.Count != 0)
                throw new ArgumentOutOfRangeException(nameof(expression), $"Some methods in the chain are not supported or are ordered in a not allowed way: {string.Join(",", callChain.Select(p=>p.Method.Name))}");

            return sb.ToString();
        }
    }
}
