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

namespace N4pper
{
    public static class IStatementRunnerExtensions
    {
        #region helpers

        internal static IDictionary<string, object> GetPropDictionary(object entity)
        {
            if (entity is IEntity)
                return ((IEntity)entity).Properties.ToDictionary(p => p.Key, p => p.Value);
            else if (entity is IDictionary<string, object>)
                return ((Dictionary<string, object>)entity);
            else
                return null;
        }
        internal static T MapEntity<T>(IEntity entity)
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
        internal static object ParseRecordValue<T>(object value, Type type)
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
        internal static IList ParseRecordsValue<T>(IList<object> value, Type type)
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
                    return new Statement(query, (IDictionary<string, object>)param);
                else
                    return new Statement(query, param.ToPropDictionary());
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
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            return new QueryableNeo4jStatement<T>(
                ext, 
                GetStatement(query, param), 
                (result, t) => 
                {
                    return ParseRecordValue<T>(result.Values[result.Keys[0]], t);
                });
        }
        public static IEnumerable<IEnumerable<T>> ExecuteQuery<T>(this IStatementRunner ext, string query, params object[] param)
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
            where T : class, new()
            where T1 : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? throw new ArgumentNullException(nameof(map));
            return null;
            //return new QueryableNeo4jStatement<T>(
            //    ext,
            //    GetStatement(query, param),
            //    result =>
            //    {
            //        return GetPropDictionary<T>(result.Values[result.Keys[0]]);
            //    });
            //return records.Select(p =>
            //    map(
            //        GetPropDictionary<T>((IEntity)p.Values[p.Keys[0]]),
            //        GetPropDictionary<T1>((IEntity)p.Values[p.Keys[1]]))
            //        ).ToList();
        }
        //public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1>(this IStatementRunner ext, string query, Func<T, T1, T> map, params object[] param)
        //    where T : class, new()
        //    where T1 : class, new()
        //{
        //    if (param == null || param.Length == 0)
        //        yield break;

        //    foreach (object item in param)
        //    {
        //        yield return ext.ExecuteQuery<T, T1>(query, map, item);
        //    }
        //}

        public static IEnumerable<T> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, string query, Func<T, T1, T2, T> map, object param = null)
            where T : class, new()
            where T1 : class, new()
            where T2 : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? throw new ArgumentNullException(nameof(map));
            return null;
            //return new QueryableNeo4jStatement<T>(
            //    ext,
            //    GetStatement(query, param),
            //    result =>
            //    {
            //        return GetPropDictionary<T>(result.Values[result.Keys[0]]);
            //    });
            //return records.Select(p =>
            //    map(
            //        GetPropDictionary<T>((IEntity)p.Values[p.Keys[0]]),
            //        GetPropDictionary<T1>((IEntity)p.Values[p.Keys[1]]),
            //        GetPropDictionary<T2>((IEntity)p.Values[p.Keys[2]]))
            //        ).ToList();
        }
        //public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2>(this IStatementRunner ext, string query, Func<T, T1, T2, T> map, params object[] param)
        //    where T : class, new()
        //    where T1 : class, new()
        //    where T2 : class, new()
        //{
        //    if (param == null || param.Length == 0)
        //        yield break;

        //    foreach (object item in param)
        //    {
        //        yield return ext.ExecuteQuery<T, T1, T2>(query, map, item);
        //    }
        //}

        //public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T> map, object param = null)
        //    where T : class, new()
        //    where T1 : class, new()
        //    where T2 : class, new()
        //    where T3 : class, new()
        //{
        //    ext = ext ?? throw new ArgumentNullException(nameof(ext));
        //    map = map ?? throw new ArgumentNullException(nameof(map));

        //    return new QueryableNeo4jStatement<T>(
        //        ext,
        //        GetStatement(query, param),
        //        result =>
        //        {
        //            return GetPropDictionary<T>(result.Values[result.Keys[0]]);
        //        });
        //    return records.Select(p =>
        //        map(
        //            GetPropDictionary<T>((IEntity)p.Values[p.Keys[0]]),
        //            GetPropDictionary<T1>((IEntity)p.Values[p.Keys[1]]),
        //            GetPropDictionary<T2>((IEntity)p.Values[p.Keys[2]]),
        //            GetPropDictionary<T3>((IEntity)p.Values[p.Keys[3]]))
        //            ).ToList();
        //}
        //public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T> map, params object[] param)
        //    where T : class, new()
        //    where T1 : class, new()
        //    where T2 : class, new()
        //    where T3 : class, new()
        //{
        //    if (param == null || param.Length == 0)
        //        yield break;

        //    foreach (object item in param)
        //    {
        //        yield return ext.ExecuteQuery<T, T1, T2, T3>(query, map, item);
        //    }
        //}

        //public static IEnumerable<T> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T4, T> map, object param = null)
        //    where T : class, new()
        //    where T1 : class, new()
        //    where T2 : class, new()
        //    where T3 : class, new()
        //    where T4 : class, new()
        //{
        //    ext = ext ?? throw new ArgumentNullException(nameof(ext));
        //    map = map ?? throw new ArgumentNullException(nameof(map));

        //    return new QueryableNeo4jStatement<T>(
        //        ext,
        //        GetStatement(query, param),
        //        result =>
        //        {
        //            return GetPropDictionary<T>(result.Values[result.Keys[0]]);
        //        });
        //    return records.Select(p =>
        //        map(
        //            GetPropDictionary<T>((IEntity)p.Values[p.Keys[0]]),
        //            GetPropDictionary<T1>((IEntity)p.Values[p.Keys[1]]),
        //            GetPropDictionary<T2>((IEntity)p.Values[p.Keys[2]]),
        //            GetPropDictionary<T3>((IEntity)p.Values[p.Keys[3]]),
        //            GetPropDictionary<T4>((IEntity)p.Values[p.Keys[4]]))
        //            ).ToList();
        //}
        //public static IEnumerable<IEnumerable<T>> ExecuteQuery<T, T1, T2, T3, T4>(this IStatementRunner ext, string query, Func<T, T1, T2, T3, T4, T> map, params object[] param)
        //    where T : class, new()
        //    where T1 : class, new()
        //    where T2 : class, new()
        //    where T3 : class, new()
        //    where T4 : class, new()
        //{
        //    if (param == null || param.Length == 0)
        //        yield break;

        //    foreach (object item in param)
        //    {
        //        yield return ext.ExecuteQuery<T, T1, T2, T3, T4>(query, map, item);
        //    }
        //}
    }
}
