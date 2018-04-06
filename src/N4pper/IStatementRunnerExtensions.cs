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

namespace N4pper
{
    public static class IStatementRunnerExtensions
    {
        #region helpers

        private static IQueryParamentersMangler ParamMangler = new DefaultParameterMangler();
        private static IRecordHandler RecordHandler = new DefaultRecordHanlder();
        
        private static Statement GetStatement(string query, object param)
        {
            query = query ?? throw new ArgumentNullException(nameof(query));

            if (param != null)
                if (param is IDictionary<string, object>)
                    return new Statement(query, ParamMangler.Mangle((IDictionary<string, object>)param));
                else
                    return new Statement(query, ParamMangler.Mangle(param.ToPropDictionary()));
            else
                return new Statement(query);
        }

        #endregion

        public static IResultSummary Execute(this IStatementRunner ext, string query, object param = null)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            return ext.Run(GetStatement(query, param))?.Summary;
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !ObjectExtensions.IsEnumerable(typeof(T)))
                    throw new InvalidOperationException("To map collections use IEnumerable`1");

                return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param), 
                (result, t) => 
                {
                    return RecordHandler.ParseRecordValue(result.Values[result.Keys[0]], typeof(T), t);
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !ObjectExtensions.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !ObjectExtensions.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                ()=>GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)RecordHandler.ParseRecordValue(result.Values[result.Keys[0]],  typeof(T), t),
                        (T1)RecordHandler.ParseRecordValue(result.Values[result.Keys[1]], typeof(T1), typeof(T1))
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !ObjectExtensions.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !ObjectExtensions.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T2)) && !ObjectExtensions.IsEnumerable(typeof(T2)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)RecordHandler.ParseRecordValue(result.Values[result.Keys[0]], typeof(T), t),
                        (T1)RecordHandler.ParseRecordValue(result.Values[result.Keys[1]], typeof(T1), typeof(T1)),
                        (T2)RecordHandler.ParseRecordValue(result.Values[result.Keys[2]], typeof(T2), typeof(T2))
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !ObjectExtensions.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !ObjectExtensions.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T2)) && !ObjectExtensions.IsEnumerable(typeof(T2)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T3)) && !ObjectExtensions.IsEnumerable(typeof(T3)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)RecordHandler.ParseRecordValue(result.Values[result.Keys[0]], typeof(T), t),
                        (T1)RecordHandler.ParseRecordValue(result.Values[result.Keys[1]], typeof(T1), typeof(T1)),
                        (T2)RecordHandler.ParseRecordValue(result.Values[result.Keys[2]], typeof(T2), typeof(T2)),
                        (T3)RecordHandler.ParseRecordValue(result.Values[result.Keys[3]], typeof(T3), typeof(T3))
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !ObjectExtensions.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !ObjectExtensions.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T2)) && !ObjectExtensions.IsEnumerable(typeof(T2)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T3)) && !ObjectExtensions.IsEnumerable(typeof(T3)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T4)) && !ObjectExtensions.IsEnumerable(typeof(T4)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)RecordHandler.ParseRecordValue(result.Values[result.Keys[0]], typeof(T), t),
                        (T1)RecordHandler.ParseRecordValue(result.Values[result.Keys[1]], typeof(T1), typeof(T1)),
                        (T2)RecordHandler.ParseRecordValue(result.Values[result.Keys[2]], typeof(T2), typeof(T2)),
                        (T3)RecordHandler.ParseRecordValue(result.Values[result.Keys[3]], typeof(T3), typeof(T3)),
                        (T4)RecordHandler.ParseRecordValue(result.Values[result.Keys[4]], typeof(T4), typeof(T4))
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
