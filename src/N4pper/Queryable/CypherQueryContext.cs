using N4pper.Queryable.CypherSintaxHelpers;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace N4pper.Queryable
{
    public class CypherQueryContext
    {
        internal static object Execute<TResult>(IStatementRunner runner, Statement statement, Func<IRecord, Type, object> mapper, Expression expression)
        {
            bool IsEnumerable = typeof(TResult).IsEnumerable();
            Type typeResult;

            QueryTranslator tranaslator = new QueryTranslator();

            expression = Evaluator.PartialEval(expression);

            string statementText = Regex.Replace(statement.Text, "RETURN", "WITH", RegexOptions.IgnoreCase);
            PipeVariableRewriter rewriter = new PipeVariableRewriter();
            statementText = rewriter.Tokenize(statementText).Rebuild();

            (string firstVar, IEnumerable<string> otherVars) = GetFirstCypherVariableName(statementText);

            string queryText = tranaslator.Translate(expression, out MethodCallExpression terminal, out int? countFromBegin, out typeResult, firstVar, otherVars);
            queryText = Regex.Replace($"{statementText} {queryText}", "RETURN", "WITH", RegexOptions.IgnoreCase);
            queryText = rewriter.Tokenize(queryText).Rebuild();
            queryText = Regex.Replace(queryText, "WITH(.+?)$", "RETURN$1", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

            IStatementResult result = runner.Run(queryText, statement.Parameters);

            IQueryable<IRecord> records = result.ToList().AsQueryable();

            if (terminal != null)
            {
                IRecord r = records.FirstOrDefault();

                if (terminal.Method.Name.StartsWith("First"))
                {
                    if (r == null)
                    {
                        if (terminal.Method.Name.EndsWith("Default"))
                            return typeResult.GetDefault();
                        else
                            throw new ArgumentOutOfRangeException(nameof(records), "The collection is empty");
                    }

                    return mapper(r, typeResult);
                }
                else
                    return Convert.ChangeType(r.Values[r.Keys[0]], terminal.Type);//can only be an aggregate numeric value
            }
            else
            {
                System.Collections.IList lst = (System.Collections.IList)typeof(List<>).MakeGenericType(typeResult).GetInstanceOf(null);
                foreach (object item in records.Select(p => mapper(p, typeResult)))
                {
                    lst.Add(item);
                }

                IQueryable lstQ = lst.AsQueryable();
                if (countFromBegin==null)
                {
                    return lstQ;
                }
                else
                {
                    ExpressionInitialModifier treeCopier = new ExpressionInitialModifier(lstQ, countFromBegin.Value);
                    Expression newExpressionTree = treeCopier.Visit(expression);

                    // This step creates an IQueryable that executes by replacing Queryable methods with Enumerable methods. 
                    if (IsEnumerable)
                        return lstQ.Provider.CreateQuery(newExpressionTree);
                    else
                        return lstQ.Provider.Execute(newExpressionTree);
                }
            }
        }
        
        private static (string, IEnumerable<string>) GetFirstCypherVariableName(string statement)
        {
            int withIdx = Regex.Match(statement, "WITH\\s+.+?$", RegexOptions.RightToLeft | RegexOptions.IgnoreCase).Index;
            string name;
            IEnumerable<string> others;

            GroupCollection gc = Regex.Match(statement.Substring(withIdx), "WITH\\s+(([^,]+,?)+)\\w?.*", RegexOptions.IgnoreCase).Groups;
            List<string> m = Regex.Matches(gc[1].Value, "[^,]+").Select(p =>
            {
                Match tmp = Regex.Match(p.Value, "AS\\s+(\\w+)", RegexOptions.IgnoreCase);
                if (tmp.Success)
                    return tmp.Groups[1].Value;
                else
                    return p.Value;
            }).ToList();
            name = m[0];
            others = m.Skip(1);

            if (name == "*")
                throw new ArgumentException($"A return variable must be specified. '*' not allowed for statement. '{statement}'", nameof(statement));

            return (name, others);
        }
    }
}
