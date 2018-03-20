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
        // Executes the expression tree that is passed to it.
        internal static object Execute<TResult>(IStatementRunner runner, Statement statement, Func<IRecord, Type, object> mapper, Expression expression)
        {
            bool IsEnumerable = TypeSystem.IsEnumerable(typeof(TResult));
            Type typeResult;

            QueryTranslator tranaslator = new QueryTranslator();

            expression = Evaluator.PartialEval(expression);

            (string firstVar, IEnumerable<string> otherVars) = GetFirstCypherVariableName(statement);

            string queryText = tranaslator.Translate(expression, out MethodCallExpression terminal, out int? countFromBegin, out typeResult, firstVar, otherVars);
                        
            IStatementResult result = runner.Run($"{Regex.Replace(statement.Text, "RETURN", "WITH", RegexOptions.IgnoreCase)} {queryText}", statement.Parameters);

            IQueryable<IRecord> records = result.ToList().AsQueryable();

            if (terminal != null)
            {
                IRecord r = records.FirstOrDefault();

                if (terminal.Method.Name.StartsWith("First"))
                {
                    if (r == null)
                    {
                        if (terminal.Method.Name.EndsWith("Default"))
                            return ObjectExtensions.GetDefault(typeResult);
                        else
                            throw new ArgumentOutOfRangeException(nameof(records), "The collection is empty");
                    }

                    return mapper(r, typeResult);
                }
                else
                    return Convert.ChangeType(r.Values[r.Keys[0]], terminal.Type);
            }
            else
            {
                System.Collections.IList lst = TypeSystem.GetListOf(typeResult);
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
        
        private static (string, IEnumerable<string>) GetFirstCypherVariableName(Statement statement)
        {
            int withIdx = Regex.Match(statement.Text, "WITH\\s+.+$", RegexOptions.RightToLeft | RegexOptions.IgnoreCase).Index;
            int returnIdx = Regex.Match(statement.Text, "RETURN\\s+.+$", RegexOptions.RightToLeft | RegexOptions.IgnoreCase).Index;
            string name;
            IEnumerable<string> others;
            if (withIdx>returnIdx)
            {
                GroupCollection gc = Regex.Match(statement.Text.Substring(withIdx), "RETURN\\s+(([^,]+,?)+)\\w?.*", RegexOptions.IgnoreCase).Groups;
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
            }
            else
            {
                GroupCollection gc = Regex.Match(statement.Text.Substring(withIdx), "RETURN\\s+(([^,]+,?)+)\\w?.*", RegexOptions.IgnoreCase).Groups;
                List<string> m = Regex.Matches(gc[1].Value, "[^,]+").Select(p=> 
                {
                    Match tmp = Regex.Match(p.Value, "AS\\s+(\\w+)", RegexOptions.IgnoreCase);
                    if (tmp.Success)
                        return tmp.Groups[1].Value;
                    else
                        return p.Value;
                }).ToList();
                name = m[0];
                others = m.Skip(1);
            }

            if (name == "*")
                throw new ArgumentException($"A return variable must be specified. '*' not allowed for statement. '{statement.Text}'", nameof(statement));

            return (name, others);
        }
    }
}
