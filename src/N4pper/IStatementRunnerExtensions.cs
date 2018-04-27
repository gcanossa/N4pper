using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N4pper.Decorators;
using System.Text;
using System.Threading.Tasks;
using OMnG;
using N4pper.Queryable;
using System.Reflection;
using System.Collections;
using qu = N4pper.QueryUtils;
using System.Threading;
using System.Linq.Expressions;
using N4pper.QueryUtils;
using N4pper.Entities;

namespace N4pper
{
    public static class IStatementRunnerExtensions
    {
        #region helpers

        private static Statement GetStatement(string query, object param, IStatementRunner runner)
        {
            query = query ?? throw new ArgumentNullException(nameof(query));

            if (param != null)
                return new Statement(query, param.ToPropDictionary());
            else
                return new Statement(query);
        }

        #endregion

        #region asynchronizers
        public static Task AsAsync(this IStatementRunner ext, Action<IStatementRunner> operation)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            operation = operation ?? throw new ArgumentNullException(nameof(operation));

            return Task.Run(() => operation(ext));
        }
        public static Task AsAsync(this IStatementRunner ext, Action<IStatementRunner> operation, CancellationToken cancellationToken)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            operation = operation ?? throw new ArgumentNullException(nameof(operation));

            return Task.Run(() => operation(ext), cancellationToken);
        }
        public static Task<T> AsAsync<T>(this IStatementRunner ext, Func<IStatementRunner, T> operation)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            operation = operation ?? throw new ArgumentNullException(nameof(operation));

            return Task.Run<T>(() => operation(ext));
        }
        public static Task<T> AsAsync<T>(this IStatementRunner ext, Func<IStatementRunner, T> operation, CancellationToken cancellationToken)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            operation = operation ?? throw new ArgumentNullException(nameof(operation));

            return Task.Run<T>(() => operation(ext), cancellationToken);
        }
        #endregion

        #region Entities CRUD
        public static T AddOrUpdateNode<T>(this IStatementRunner ext, T entity, Expression<Func<T, object>> propMatch = null) where T : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            entity = entity ?? throw new ArgumentNullException(nameof(entity));
            IEnumerable<string> keyOn = propMatch?.ToPropertyNameCollection() ?? new string[0];

            if(keyOn.Count()>0)
            {
                foreach (string name in keyOn)
                {
                    PropertyInfo pinfo = typeof(T).GetProperty(name);
                    if (ObjectExtensions.Configuration.Get(pinfo, entity) == pinfo.PropertyType.GetDefault())
                        throw new ArgumentException($"Every selection matching property must be set. '{name}' has its default type value.");
                }
            }

            Symbol s = new Symbol();
            Node n = new Node(s, typeof(T), entity.SelectProperties(keyOn));
            n.Parametrize(prefix:"value.");

            return ext.ExecuteQuery<T>(
                $"MERGE {n} " +
                $"ON CREATE SET {s}+=$value,{s}.{nameof(IOgmEntity.EntityId)}=id({s}) " +
                $"ON MATCH SET {s}+=$value,{s}.{nameof(IOgmEntity.EntityId)}=id({s}) " +
                $"RETURN {s}",
                new { value = entity.SelectMatchingTypesProperties(p => p.IsPrimitive() || p.IsOfGenericType(typeof(IEnumerable<>), t => t.Type.IsPrimitive())) }).FirstOrDefault();
        }
        public static IResultSummary DeleteNode<T>(this IStatementRunner ext, T entity, Expression<Func<T, object>> propMatch = null) where T : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            entity = entity ?? throw new ArgumentNullException(nameof(entity));
            IEnumerable<string> keyOn = propMatch?.ToPropertyNameCollection() ?? entity.GetType().GetProperties().Select(p=>p.Name);

            if (keyOn.Count() > 0)
            {
                foreach (string name in keyOn)
                {
                    PropertyInfo pinfo = typeof(T).GetProperty(name);
                    if (ObjectExtensions.Configuration.Get(pinfo, entity) == pinfo.PropertyType.GetDefault())
                        throw new ArgumentException($"Every selection matching property must be set. '{name}' has its default type value.");
                }
            }

            Symbol s = new Symbol();
            Node n = new Node(s, typeof(T), entity.SelectProperties(keyOn));
            n.Parametrize(prefix: "value.");

            return ext.Execute(
                $"MATCH {n.BuildForQuery()} " + 
                $"DETACH DELETE {s}",
                new { value = entity });
        }
        public static IQueryable<T> GetQueryableNodeSet<T>(this IStatementRunner ext) where T : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            Symbol s = new Symbol();
            Node n = new Node(s, typeof(T));

            return ext.ExecuteQuery<T>(
                $"MATCH {n.BuildForQuery()} " +
                $"RETURN {s}");
        }
        #endregion

        public static IResultSummary Execute(this IStatementRunner ext, string query, object param = null)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            return ext.Run(GetStatement(query, param, ext))?.Summary;
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

        public static IQueryable<T> ExecuteQuery<T>(this IStatementRunner ext, string query, object param = null) 
            where T : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            if (typeof(T).IsCollection() && !typeof(T).IsEnumerable())
                    throw new InvalidOperationException("To map collections use IEnumerable`1");

                return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param, ext), 
                (result, t) => 
                {
                    return ext.AsManaged().ParseRecord(result.Values[result.Keys[0]], typeof(T), t);
                });
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T>(this IStatementRunner ext, string query, params object[] param)
            where T : class
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

        public static IEnumerable<T> ExecuteQuery<T, T1>(this IStatementRunner ext, string query, Func<T, T1, T> map, object param = null)
            where T : class
            where T1 : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? throw new ArgumentNullException(nameof(map));

            if (typeof(T).IsCollection() && !typeof(T).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T1).IsCollection() && !typeof(T1).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                ()=>GetStatement(query, param, ext),
                (result, t) =>
                {
                    return map(
                        (T)ext.AsManaged().ParseRecord(result.Values[result.Keys[0]],  typeof(T), t),
                        (T1)ext.AsManaged().ParseRecord(result.Values[result.Keys[1]], typeof(T1), typeof(T1))
                        );
                });
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1>(this IStatementRunner ext, string query, Func<T, T1, T> map, params object[] param)
            where T : class
            where T1 : class
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1>(query, map, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, string query, Func<T, T1, T2, T> map, object param = null)
            where T : class
            where T1 : class
            where T2 : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? throw new ArgumentNullException(nameof(map));

            if (typeof(T).IsCollection() && !typeof(T).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T1).IsCollection() && !typeof(T1).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T2).IsCollection() && !typeof(T2).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param, ext),
                (result, t) =>
                {
                    return map(
                        (T)ext.AsManaged().ParseRecord(result.Values[result.Keys[0]], typeof(T), t),
                        (T1)ext.AsManaged().ParseRecord(result.Values[result.Keys[1]], typeof(T1), typeof(T1)),
                        (T2)ext.AsManaged().ParseRecord(result.Values[result.Keys[2]], typeof(T2), typeof(T2))
                        );
                });
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, string query, Func<T, T1, T2, T> map, params object[] param)
            where T : class
            where T1 : class
            where T2 : class
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1, T2>(query, map, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T> map, object param = null)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? throw new ArgumentNullException(nameof(map));

            if (typeof(T).IsCollection() && !typeof(T).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T1).IsCollection() && !typeof(T1).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T2).IsCollection() && !typeof(T2).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T3).IsCollection() && !typeof(T3).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param, ext),
                (result, t) =>
                {
                    return map(
                        (T)ext.AsManaged().ParseRecord(result.Values[result.Keys[0]], typeof(T), t),
                        (T1)ext.AsManaged().ParseRecord(result.Values[result.Keys[1]], typeof(T1), typeof(T1)),
                        (T2)ext.AsManaged().ParseRecord(result.Values[result.Keys[2]], typeof(T2), typeof(T2)),
                        (T3)ext.AsManaged().ParseRecord(result.Values[result.Keys[3]], typeof(T3), typeof(T3))
                        );
                });
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T> map, params object[] param)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1, T2, T3>(query, map, item);
            }
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T4, T> map, object param = null)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? throw new ArgumentNullException(nameof(map));

            if (typeof(T).IsCollection() && !typeof(T).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T1).IsCollection() && !typeof(T1).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T2).IsCollection() && !typeof(T2).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T3).IsCollection() && !typeof(T3).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (typeof(T4).IsCollection() && !typeof(T4).IsEnumerable())
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param, ext),
                (result, t) =>
                {
                    return map(
                        (T)ext.AsManaged().ParseRecord(result.Values[result.Keys[0]], typeof(T), t),
                        (T1)ext.AsManaged().ParseRecord(result.Values[result.Keys[1]], typeof(T1), typeof(T1)),
                        (T2)ext.AsManaged().ParseRecord(result.Values[result.Keys[2]], typeof(T2), typeof(T2)),
                        (T3)ext.AsManaged().ParseRecord(result.Values[result.Keys[3]], typeof(T3), typeof(T3)),
                        (T4)ext.AsManaged().ParseRecord(result.Values[result.Keys[4]], typeof(T4), typeof(T4))
                        );
                });
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T4, T> map, params object[] param)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            if (param == null || param.Length == 0)
                yield break;

            foreach (object item in param)
            {
                yield return ext.ExecuteQuery<T, T1, T2, T3, T4>(query, map, item);
            }
        }

        #region query builder overrides
        public static IResultSummary Execute(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, object param = null)
        {
            return ext.Execute(query(new qu.QueryBuilder()), param);
        }
        public static IEnumerable<IResultSummary> Execute(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, params object[] param)
        {
            return ext.Execute(query(new qu.QueryBuilder()), param);
        }

        public static IQueryable<T> ExecuteQuery<T>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, object param = null)
            where T : class
        {
            return ext.ExecuteQuery<T>(query(new qu.QueryBuilder()),param);
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, params object[] param)
            where T : class
        {
            return ext.ExecuteQuery<T>(query(new qu.QueryBuilder()), param);
        }

        public static IEnumerable<T> ExecuteQuery<T, T1>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T> map, object param = null)
            where T : class
            where T1 : class
        {
            return ext.ExecuteQuery<T, T1>(query(new qu.QueryBuilder()), map, param);
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T> map, params object[] param)
            where T : class
            where T1 : class
        {
            return ext.ExecuteQuery<T, T1>(query(new qu.QueryBuilder()), map, param);
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T2, T> map, object param = null)
            where T : class
            where T1 : class
            where T2 : class
        {
            return ext.ExecuteQuery<T, T1,T2>(query(new qu.QueryBuilder()), map, param);
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T2, T> map, params object[] param)
            where T : class
            where T1 : class
            where T2 : class
        {
            return ext.ExecuteQuery<T, T1, T2>(query(new qu.QueryBuilder()), map, param);
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T2, T3, T> map, object param = null)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
        {
            return ext.ExecuteQuery<T, T1, T2,T3>(query(new qu.QueryBuilder()), map, param);
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T2, T3, T> map, params object[] param)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
        {
            return ext.ExecuteQuery<T, T1, T2, T3>(query(new qu.QueryBuilder()), map, param);
        }

        public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T2, T3, T4, T> map, object param = null)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return ext.ExecuteQuery<T, T1, T2, T3, T4>(query(new qu.QueryBuilder()), map, param);
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, Func<qu.IQueryBuilder, string> query, Func<T, T1, T2, T3, T4, T> map, params object[] param)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return ext.ExecuteQuery<T, T1, T2, T3, T4>(query(new qu.QueryBuilder()), map, param);
        }
        #endregion
    }
}
