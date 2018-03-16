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
        internal static object Execute<TResult>(IStatementRunner runner, Statement statement, Func<IRecord, IDictionary<string, object>> mapper, Expression expression)
        {
            // The expression must represent a query over the data source. 
            if (!IsQueryOverDataSource(expression))
                throw new InvalidProgramException("No query over the data source was specified.");

            bool IsEnumerable = TypeSystem.IsEnumerable(typeof(TResult));
            Type typeResult = IsEnumerable ? typeof(TResult).GetGenericArguments()[0] : typeof(TResult);

            QueryTranslator tranaslator = new QueryTranslator();

            expression = Evaluator.PartialEval(expression);

            (string firstVar, IEnumerable<string> otherVars) = GetFirstCypherVariableName(statement);

            string queryText = tranaslator.Translate(expression, out MethodCallExpression terminal, firstVar, otherVars);

            IStatementResult result = runner.Run($"{statement.Text.Replace("RETURN","WITH")} {queryText}", statement.Parameters);

            //TODO: da aggiungere il Map<T>
            IQueryable<IRecord> records = result.ToList().AsQueryable();

            if (terminal != null)
            {
                IRecord r = records.FirstOrDefault();

                if (ObjectExtensions.IsPrimitive(terminal.Method.ReturnType))
                {
                    return Convert.ChangeType(r.Values[r.Keys[0]], typeof(TResult));
                }
                else if (r == null)
                {
                    if (terminal.Method.Name.EndsWith("Default"))
                        return null;
                    else
                        throw new ArgumentOutOfRangeException(nameof(records), "The collection is empty");
                }

                return GetInstanceOf(typeResult, mapper(r));
            }
            else
            {
                System.Collections.IList lst = GetListOf(typeResult);
                foreach (object item in records.Select(p => GetInstanceOf(typeResult, mapper(p))))
                {
                    lst.Add(item);
                }
                
                return lst.AsQueryable();
            }

            //MethodCallExpression e = tranaslator.GetExpressionsCallChain(expression).First();

            //ExpressionInitialCollectionModifier modifier = new ExpressionInitialCollectionModifier(records, tranaslator.GetExpressionsCallChain(expression).First());
            //expression = modifier.Visit(expression);
 
            //if (IsEnumerable)
            //    return records.Provider.CreateQuery(expression);
            //else
            //    return records.Provider.Execute(expression);
        }

        private static object GetInstanceOf(Type type, IDictionary<string, object> param)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            param = param ?? new Dictionary<string, object>();
            
            object[] args = GetParamsForConstructor(type, param);
            if(args != null)
                return args.Length == 0 ?
                        Activator.CreateInstance(type, args).CopyProperties(param):
                        Activator.CreateInstance(type, args);

            throw new Exception("Unable to build an object of the desired type");
        }

        private static object[] GetParamsForConstructor(Type type, IDictionary<string, object> param)
        {
            if (type.GetConstructor(new Type[0]) != null)
                return new object[0];
            else
            {
                List<object> result = new List<object>();
                foreach (ConstructorInfo item in type.GetConstructors())
                {
                    List<ParameterInfo> tmp = item.GetParameters().ToList();
                    if (tmp.Count == param.Keys.Count)
                    {
                        result.Clear();
                        foreach (ParameterInfo pinfo in tmp.ToList())
                        {
                            if (param.ContainsKey(pinfo.Name))
                            {
                                tmp.Remove(pinfo);
                                result.Add(Convert.ChangeType(param[pinfo.Name], pinfo.ParameterType));
                            }
                        }
                        if (tmp.Count == 0)
                            return result.ToArray();
                    }
                }

                return null;
            }
        }

        private static System.Collections.IList GetListOf(Type type)
        {
            Type lst = typeof(List<>).MakeGenericType(type);
            return (System.Collections.IList)Activator.CreateInstance(lst);
        }

        private static (string, IEnumerable<string>) GetFirstCypherVariableName(Statement statement)
        {
            int withIdx = Regex.Match(statement.Text, "WITH\\s+.+$", RegexOptions.RightToLeft).Index;
            int returnIdx = Regex.Match(statement.Text, "RETURN\\s+.+$", RegexOptions.RightToLeft).Index;
            string name;
            IEnumerable<string> others;
            if (withIdx>returnIdx)
            {
                GroupCollection gc = Regex.Match(statement.Text.Substring(withIdx), "WITH\\s+(([^,\\s]+,?)+)\\w?.*").Groups;
                List<Match> m = Regex.Matches(gc[1].Value, "[^,\\s]+").ToList();
                name = m[0].Value;
                others = m.Skip(1).Select(p => p.Value);
            }
            else
            {
                GroupCollection gc = Regex.Match(statement.Text.Substring(withIdx), "RETURN\\s+(([^,\\s]+,?)+)\\w?.*").Groups;
                List<Match> m = Regex.Matches(gc[1].Value, "[^,\\s]+").ToList();
                name = m[0].Value;
                others = m.Skip(1).Select(p=>p.Value);
            }

            if (name == "*")
                throw new ArgumentException($"A return variable must be specified. '*' not allowed for statement. '{statement.Text}'", nameof(statement));

            return (name, others);
        }

        private static bool IsQueryOverDataSource(Expression expression)
        {
            // If expression represents an unqueried IQueryable data source instance, 
            // expression is of type ConstantExpression, not MethodCallExpression. 
            return (expression is MethodCallExpression);
        }
    }
}
