using AsIKnow.Graph;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N4pper.Decorators;
using System.Text;
using System.Threading.Tasks;

namespace N4pper
{
    public static class IStatementRunnerExtensions
    {
        //TODO: add async support, cause of Neo4J.Driver ConsumingEnumerables has to be used.
        #region helpers

        private static GraphEntity Map(object obj, GraphManager manager)
        {
            if (obj is INode)
                return MapNode((INode)obj, manager);
            else if (obj is IRelationship)
                return MapRelationship((IRelationship)obj, manager);
            else
                throw new ArgumentException($"Unable to map type {obj.GetType().FullName}", nameof(obj));
        }
        private static Node MapNode(INode node, GraphManager manager)
        {
            Node result = manager.CreateNode();

            manager.Manager.GetTypesFromLabels(node.Labels).ToList().ForEach(p => result.AddLabel(p));

            result.SetProps(node.Properties);

            return result;
        }
        private static Relationship MapRelationship(IRelationship relationship, GraphManager manager)
        {
            Relationship result = manager.CreateRelationship();

            result.OfType(manager.Manager.GetTypesFromLabels(new[] { relationship.Type }).First());

            result.SetProps(relationship.Properties);

            return result;
        }

        private static IStatementResult GetResult(IStatementRunner ext, string query, object param)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            GraphManager mgr = (ext as IGraphManagedStatementRunner)?.Manager?.Manager ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            IStatementResult result;
            if (param != null)
                result = ext.Run(query, param);
            else
                result = ext.Run(query);

            return result;
        }

        #endregion

        public static IResultSummary Execute<T>(this IStatementRunner ext, string query, object param = null) where T : class, new()
        {
            return GetResult(ext, query, param).Summary;
        }
        public static IEnumerable<IResultSummary> Execute<T>(this IStatementRunner ext, string query, params object[] param) where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            if (param == null || param.Length == 0)
                yield break;
                        
            foreach (object item in param)
            {
                yield return ext.Execute<T>(query, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T>(this IStatementRunner ext, string query, object param = null) where T : class, new()
        {
            IStatementResult result = GetResult(ext, query, param);
            List<IRecord> records = result.ToList();

            GraphManager mgr = (ext as IGraphManagedStatementRunner)?.Manager?.Manager;

            if (result.Keys.Count < 1)
                throw new Exception("The query did not produced enough results");

            return records.Select(p => Map(p.Values[p.Keys[0]], mgr).FillObject<T>()).ToList();
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

            GraphManager mgr = (ext as IGraphManagedStatementRunner)?.Manager?.Manager;

            if (result.Keys.Count < 2)
                throw new Exception("The query did not produced enough results");

            return records.Select(p => 
                map(
                    Map(p.Values[p.Keys[0]], mgr).FillObject<T>(), 
                    Map(p.Values[p.Keys[1]], mgr).FillObject<T1>())
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

            GraphManager mgr = (ext as IGraphManagedStatementRunner)?.Manager?.Manager;

            if (result.Keys.Count < 3)
                throw new Exception("The query did not produced enough results");

            return records.Select(p =>
                map(
                    Map(p.Values[p.Keys[0]], mgr).FillObject<T>(),
                    Map(p.Values[p.Keys[1]], mgr).FillObject<T1>(),
                    Map(p.Values[p.Keys[2]], mgr).FillObject<T2>())
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

            GraphManager mgr = (ext as IGraphManagedStatementRunner)?.Manager?.Manager;

            if (result.Keys.Count < 4)
                throw new Exception("The query did not produced enough results");

            return records.Select(p =>
                map(
                    Map(p.Values[p.Keys[0]], mgr).FillObject<T>(),
                    Map(p.Values[p.Keys[1]], mgr).FillObject<T1>(),
                    Map(p.Values[p.Keys[2]], mgr).FillObject<T2>(),
                    Map(p.Values[p.Keys[3]], mgr).FillObject<T3>())
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

            GraphManager mgr = (ext as IGraphManagedStatementRunner)?.Manager?.Manager;

            if (result.Keys.Count < 5)
                throw new Exception("The query did not produced enough results");

            return records.Select(p =>
                map(
                    Map(p.Values[p.Keys[0]], mgr).FillObject<T>(),
                    Map(p.Values[p.Keys[1]], mgr).FillObject<T1>(),
                    Map(p.Values[p.Keys[2]], mgr).FillObject<T2>(),
                    Map(p.Values[p.Keys[3]], mgr).FillObject<T3>(),
                    Map(p.Values[p.Keys[4]], mgr).FillObject<T4>())
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
