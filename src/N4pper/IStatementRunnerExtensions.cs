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

        internal static Dictionary<string, object> FixParameters(IDictionary<string, object> param)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (param == null)
                return result;

            foreach (KeyValuePair<string, object> kv in param.Where(p=>p.Value == null || ObjectExtensions.IsPrimitive(p.Value.GetType())))
            {
                if (kv.Value == null)
                    result.Add(kv.Key, kv.Value);
                else if (kv.Value.IsDateTime())
                {
                    DateTimeOffset d = (DateTime)kv.Value;
                    result.Add(kv.Key, d.ToUnixTimeMilliseconds());
                }
                else if (kv.Value.GetType() == typeof(TimeSpan) || kv.Value.GetType() == typeof(TimeSpan?))
                    result.Add(kv.Key, ((TimeSpan)kv.Value).TotalMilliseconds);
                else
                    result.Add(kv.Key, kv.Value);
            }

            return result;
        }
        internal static IDictionary<string, object> GetPropDictionary(object entity)
        {
            if (entity is IEntity)
                return ((IEntity)entity).Properties.ToDictionary(p => p.Key, p => p.Value);
            else if (entity is IDictionary<string, object>)
                return ((Dictionary<string, object>)entity);
            else
                return null;
        }
        internal static T MapEntity<T>(IEntity entity) where T : class
        {
            object obj = null;
            if(entity is INode)
            {
                obj = ((INode)entity).Labels.GetTypesFromLabels(new N4pperTypeExtensionsConfiguration()).GetInstanceOfMostSpecific();
            }
            else if(entity is IRelationship)
            {
                obj = new string[] { ((IRelationship)entity).Type }.GetTypesFromLabels(new N4pperTypeExtensionsConfiguration()).GetInstanceOfMostSpecific();
            }

            return ((T)obj).CopyProperties(entity.Properties.ToDictionary(p=>p.Key,p=>p.Value));
        }
        internal static object ParseRecordValue<T>(object value, Type type) where T : class
        {
            if (ObjectExtensions.IsCollection(typeof(T)))
            {
                if (!TypeSystem.IsEnumerable(typeof(T)))
                    throw new InvalidOperationException("To map collections use IEnumerable`1");

                MethodInfo m = typeof(IStatementRunnerExtensions).GetMethod($"{nameof(ParseRecordsValue)}", BindingFlags.NonPublic | BindingFlags.Static);
                m = m.MakeGenericMethod(typeof(T).GetGenericArguments());
                return m.Invoke(null, new object[] { value, type.GetGenericArguments()[0] });
            }
            else
            {
                if (value is IList<object>)
                    return ParseRecordsValue<T>((IList<object>)value, type);
                if (value is IEntity && typeof(T).IsAssignableFrom(type))
                    return MapEntity<T>((IEntity)value);
                else
                    return ObjectExtensions.GetInstanceOf(type, GetPropDictionary(value));
            }
        }
        internal static IList ParseRecordsValue<T>(IList<object> value, Type type) where T : class
        {
            IList lst = TypeSystem.GetListOf(type);
            foreach (object item in value.Select(p => ParseRecordValue<T>(p, type)))
            {
                lst.Add(item);
            }
            return lst;
        }
        
        internal static Statement GetStatement(string query, object param)
        {
            query = query ?? throw new ArgumentNullException(nameof(query));

            if (param != null)
                if (param is IDictionary<string, object>)
                    return new Statement(query, FixParameters((IDictionary<string, object>)param));
                else
                    return new Statement(query, FixParameters(param.ToPropDictionary()));
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !TypeSystem.IsEnumerable(typeof(T)))
                    throw new InvalidOperationException("To map collections use IEnumerable`1");

                return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param), 
                (result, t) => 
                {
                    return ParseRecordValue<T>(result.Values[result.Keys[0]], t);
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !TypeSystem.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !TypeSystem.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                ()=>GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)ParseRecordValue<T>(result.Values[result.Keys[0]], t),
                        (T1)ParseRecordValue<T1>(result.Values[result.Keys[1]], typeof(T1))
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !TypeSystem.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !TypeSystem.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T2)) && !TypeSystem.IsEnumerable(typeof(T2)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)ParseRecordValue<T>(result.Values[result.Keys[0]], t),
                        (T1)ParseRecordValue<T1>(result.Values[result.Keys[1]], typeof(T1)),
                        (T2)ParseRecordValue<T2>(result.Values[result.Keys[2]], typeof(T2))
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !TypeSystem.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !TypeSystem.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T2)) && !TypeSystem.IsEnumerable(typeof(T2)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T3)) && !TypeSystem.IsEnumerable(typeof(T3)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)ParseRecordValue<T>(result.Values[result.Keys[0]], t),
                        (T1)ParseRecordValue<T1>(result.Values[result.Keys[1]], typeof(T1)),
                        (T2)ParseRecordValue<T2>(result.Values[result.Keys[2]], typeof(T2)),
                        (T3)ParseRecordValue<T3>(result.Values[result.Keys[3]], typeof(T3))
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

            if (ObjectExtensions.IsCollection(typeof(T)) && !TypeSystem.IsEnumerable(typeof(T)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T1)) && !TypeSystem.IsEnumerable(typeof(T1)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T2)) && !TypeSystem.IsEnumerable(typeof(T2)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T3)) && !TypeSystem.IsEnumerable(typeof(T3)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");
            if (ObjectExtensions.IsCollection(typeof(T4)) && !TypeSystem.IsEnumerable(typeof(T4)))
                throw new InvalidOperationException("To map collections use IEnumerable`1");

            return new QueryableNeo4jStatement<T>(
                ext,
                () => GetStatement(query, param),
                (result, t) =>
                {
                    return map(
                        (T)ParseRecordValue<T>(result.Values[result.Keys[0]], t),
                        (T1)ParseRecordValue<T1>(result.Values[result.Keys[1]], typeof(T1)),
                        (T2)ParseRecordValue<T2>(result.Values[result.Keys[2]], typeof(T2)),
                        (T3)ParseRecordValue<T3>(result.Values[result.Keys[3]], typeof(T3)),
                        (T4)ParseRecordValue<T4>(result.Values[result.Keys[4]], typeof(T4))
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
