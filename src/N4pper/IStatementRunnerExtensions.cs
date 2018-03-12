using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N4pper.Decorators;
using System.Text;
using System.Threading.Tasks;
using OMnG;

namespace N4pper
{
    public static class IStatementRunnerExtensions
    {
        //TODO: add async support. Because of Neo4J.Driver, ConsumingEnumerables has to be used.
        #region helpers
        
        private static T Map<T>(IEntity entity) where T : class, new()
        {
            return new T().CopyProperties(entity.Properties.ToDictionary(p => p.Key, p => p.Value));
        }
        
        private static IStatementResult GetResult(IStatementRunner ext, string query, object param)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            IStatementResult result;
            if (param != null)
                if(param is IDictionary<string, object>)
                    result = ext.Run(query, (IDictionary<string, object>)param);
                else
                    result = ext.Run(query, param);
            else
                result = ext.Run(query);

            return result;
        }

        #endregion

        public static IResultSummary Execute(this IStatementRunner ext, string query, object param = null)
        {
            return GetResult(ext, query, param).Summary;
        }
        public static IEnumerable<IResultSummary> Execute(this IStatementRunner ext, string query, params object[] param)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            if (param == null || param.Length == 0)
                yield break;
                        
            foreach (object item in param)
            {
                yield return ext.Execute(query, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T>(this IStatementRunner ext, string query, object param = null) where T : class, new()
        {
            IStatementResult result = GetResult(ext, query, param);
            List<IRecord> records = result.ToList();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner);

            if (result.Keys.Count < 1)
                throw new Exception("The query did not produced enough results");
            
            return records.Select(p => Map<T>((IEntity)p.Values[p.Keys[0]])).ToList();
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T>(this IStatementRunner ext, string query, params object[] param) where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            if (param == null || param.Length == 0)
                yield break;
            
            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T>(query, item);
            }
        }
        
        public static IEnumerable<T> ExecuteQuery<T, T1>(this IStatementRunner ext, string query, Func<T,T1,T> map, object param = null) 
            where T : class, new()
            where T1 : class, new()
        {
            map = map ?? throw new ArgumentNullException(nameof(map));
            
            IStatementResult result = GetResult(ext, query, param);
            List<IRecord> records = result.ToList();

            if (result.Keys.Count < 2)
                throw new Exception("The query did not produced enough results");

            return records.Select(p => 
                map(
                    Map<T>((IEntity)p.Values[p.Keys[0]]), 
                    Map<T1>((IEntity)p.Values[p.Keys[1]]))
                    ).ToList();
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1>(this IStatementRunner ext, string query, Func<T, T1, T> map, params object[] param)
            where T : class, new()
            where T1 : class, new()
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1>(query, map, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, string query, Func<T, T1, T2, T> map, object param = null)
            where T : class, new()
            where T1 : class, new()
            where T2 : class, new()
        {
            map = map ?? throw new ArgumentNullException(nameof(map));

            IStatementResult result = GetResult(ext, query, param);
            List<IRecord> records = result.ToList();

            if (result.Keys.Count < 3)
                throw new Exception("The query did not produced enough results");

            return records.Select(p =>
                map(
                    Map<T>((IEntity)p.Values[p.Keys[0]]),
                    Map<T1>((IEntity)p.Values[p.Keys[1]]),
                    Map<T2>((IEntity)p.Values[p.Keys[2]]))
                    ).ToList();
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, string query, Func<T, T1, T2, T> map, params object[] param)
            where T : class, new()
            where T1 : class, new()
            where T2 : class, new()
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1, T2>(query, map, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T> map, object param = null)
            where T : class, new()
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
        {
            map = map ?? throw new ArgumentNullException(nameof(map));

            IStatementResult result = GetResult(ext, query, param);
            List<IRecord> records = result.ToList();
            
            if (result.Keys.Count < 4)
                throw new Exception("The query did not produced enough results");

            return records.Select(p =>
                map(
                    Map<T>((IEntity)p.Values[p.Keys[0]]),
                    Map<T1>((IEntity)p.Values[p.Keys[1]]),
                    Map<T2>((IEntity)p.Values[p.Keys[2]]),
                    Map<T3>((IEntity)p.Values[p.Keys[3]]))
                    ).ToList();
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T> map, params object[] param)
            where T : class, new()
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1, T2, T3>(query, map, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T4, T> map, object param = null)
            where T : class, new()
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
        {
            map = map ?? throw new ArgumentNullException(nameof(map));

            IStatementResult result = GetResult(ext, query, param);
            List<IRecord> records = result.ToList();

            if (result.Keys.Count < 5)
                throw new Exception("The query did not produced enough results");

            return records.Select(p =>
                map(
                    Map<T>((IEntity)p.Values[p.Keys[0]]),
                    Map<T1>((IEntity)p.Values[p.Keys[1]]),
                    Map<T2>((IEntity)p.Values[p.Keys[2]]),
                    Map<T3>((IEntity)p.Values[p.Keys[3]]),
                    Map<T4>((IEntity)p.Values[p.Keys[4]]))
                    ).ToList();
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T4, T> map, params object[] param)
            where T : class, new()
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1, T2, T3, T4>(query, map, item);
            }
        }
    }
}
