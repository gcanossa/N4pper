using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable
{
    internal class QueryTranslator
    {
        internal QueryTranslator()
        {
        }

        public string WherePart { get; set; } = "";
        public string SelectPart { get; set; } = "";
        public string ReturnPart { get; set; } = "";
        public string OrderByPart { get; set; } = "";
        public string SkipPart { get; set; } = "";
        public string LimitPart { get; set; } = "";

        internal Type GetExpressionsTypeResult(Expression expression)
        {
            Type typeResult = TypeSystem.GetElementType(expression.Type);

            while (expression is MethodCallExpression &&
                ((MethodCallExpression)expression).Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                typeResult = TypeSystem.GetElementType(expression.Type);
                expression = ((MethodCallExpression)expression).Arguments[0];
            }

            return typeResult;
        }

        internal List<MethodCallExpression> GetExpressionsCallChain(Expression expression)
        {
            List<MethodCallExpression>  callChain = new List<MethodCallExpression>();

            while (expression is MethodCallExpression &&
                ((MethodCallExpression)expression).Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                callChain.Add((MethodCallExpression)expression);
                expression = ((MethodCallExpression)expression).Arguments[0];
            }

            callChain.Reverse();

            return callChain;
        }
        
        private void VisitWhereStatements(List<MethodCallExpression> callChain, Type typeResult)
        {
            string[] names = new[] { nameof(q.Where), nameof(q.Distinct) };
            List<MethodCallExpression> tmp = callChain.Where(p => names.Contains(p.Method.Name)).ToList();
            
            WhereQueryTranslator s = new WhereQueryTranslator(typeResult);
            foreach (MethodCallExpression item in tmp)
            {
                s.Accept(item);
                callChain.Remove(item);
            }

            WherePart = s.Statement;
        }
        private void VisitSelectStatements(List<MethodCallExpression> callChain, Type typeResult, IEnumerable<string> otherVariables)
        {
            string[] names = new[] { nameof(q.Select) };
            List<MethodCallExpression> tmp = callChain.Where(p => names.Contains(p.Method.Name)).ToList();

            SelectQueryTranslator s = new SelectQueryTranslator(typeResult, otherVariables);
            foreach (MethodCallExpression item in tmp)
            {
                s.Accept(item);
                callChain.Remove(item);
            }

            SelectPart = s.Statement;
        }
        private void VisitOrderByStatements(List<MethodCallExpression> callChain, Type typeResult)
        {
            string[] names = new[] { nameof(q.OrderBy), nameof(q.ThenBy), nameof(q.OrderByDescending), nameof(q.ThenByDescending) };

            List<MethodCallExpression> tmp = callChain.Where(p => names.Contains(p.Method.Name)).ToList();
            
            OrderByQueryTranslator s = new OrderByQueryTranslator(typeResult);
            foreach (MethodCallExpression item in tmp)
            {
                s.Accept(item);
                callChain.Remove(item);
            }

            if(!string.IsNullOrEmpty(s.Statement))
                OrderByPart = $" ORDER BY {s.Statement}";
        }
        private void VisitSkipStatements(List<MethodCallExpression> callChain)
        {
            List<MethodCallExpression> tmp = callChain.Where(p => p.Method.Name == nameof(q.Skip)).ToList();
            
            SkipParamSelector s = new SkipParamSelector();
            foreach (MethodCallExpression item in tmp)
            {
                s.Accept(item);
                callChain.Remove(item);
            }

            if(s.SkipParam>0)
                SkipPart = $" SKIP {s.SkipParam}";
        }
        private void VisitLimitStatements(List<MethodCallExpression> callChain)
        {
            List<MethodCallExpression> tmp = callChain.Where(p => p.Method.Name == nameof(q.Take)).ToList();

            TakeParamSelector s = new TakeParamSelector();
            foreach (MethodCallExpression item in tmp)
            {
                s.Accept(item);
                callChain.Remove(item);
            }

            if(s.TakeParam>0)
                LimitPart = $" LIMIT {s.TakeParam}";
        }
        private MethodCallExpression VisitAggregateStatements(List<MethodCallExpression> callChain, Type typeResult)
        {
            string[] names = new[] { nameof(q.Count), nameof(q.Average), nameof(q.Sum), nameof(q.Min), nameof(q.Max) };

            MethodCallExpression tmp = callChain.FirstOrDefault(p => names.Contains(p.Method.Name));

            if (tmp == null)
            {
                ReturnPart = " RETURN *";
                return null;
            }
            else
            {
                AggregatedQueryTranslator s = new AggregatedQueryTranslator(typeResult);
                s.Accept(tmp);
                callChain.Remove(tmp);

                ReturnPart = s.Statement;

                return tmp;
            }
        }
        private MethodCallExpression VisitFirstStatements(List<MethodCallExpression> callChain, Type typeResult, string varName, IEnumerable<string> otherVariables)
        {
            string[] names = new[] { nameof(q.First), nameof(q.FirstOrDefault) };

            MethodCallExpression tmp = callChain.FirstOrDefault(p => names.Contains(p.Method.Name));
            
            if (tmp != null)
            {
                FirstQueryTranslator s = new FirstQueryTranslator(typeResult);
                s.Accept(tmp);
                callChain.Remove(tmp);

                ReturnPart = $"{s.Statement} RETURN {varName}";
                if(otherVariables.Count()>0)
                    ReturnPart += $",{string.Join(",", otherVariables)}";
                ReturnPart += " LIMIT 1";
                return tmp;
            }
            return null;
        }
        internal string Translate(Expression expression, out MethodCallExpression terminalExpr, string paramNameOverride, IEnumerable<string> otherVariables)
        {
            otherVariables = otherVariables ?? new string[0];
            Type typeResult = GetExpressionsTypeResult(expression);

            ParameterNameRewriter pr = new ParameterNameRewriter(paramNameOverride, typeResult);
            expression = pr.Visit(expression);

            List<MethodCallExpression> callChain = GetExpressionsCallChain(expression);

            StringBuilder sb = new StringBuilder();

            VisitWhereStatements(callChain, typeResult);
            VisitOrderByStatements(callChain, typeResult);
            VisitSelectStatements(callChain, typeResult, otherVariables);
            VisitSkipStatements(callChain);
            VisitLimitStatements(callChain);
            terminalExpr = VisitAggregateStatements(callChain, typeResult);
            if(terminalExpr==null)
                terminalExpr = VisitFirstStatements(callChain, typeResult, paramNameOverride, otherVariables);

            sb.Append(WherePart);
            sb.Append(OrderByPart);
            sb.Append(SelectPart);
            sb.Append(SkipPart);
            sb.Append(LimitPart);
            sb.Append(ReturnPart);

            return sb.ToString();
        }
    }
}
