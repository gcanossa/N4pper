using N4pper.Orm.Design;
using Neo4j.Driver.V1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Orm
{
    internal class GraphContextQueryHelpers
    {
        private static MethodInfo _ParseRecordValue = typeof(IStatementRunnerExtensions).GetMethod("ParseRecordValue");
        public static object Map(IRecord record, Type type)
        {
            List<object> pool = new List<object>();
            return RecursiveMap(record[1] as IDictionary<string, object>, type, pool);
        }
        private static object RecursiveMap(IDictionary<string, object> record, Type type, List<object> objectPool)
        {
            //TODO: resuse existing objects don't create duplicates
            if (record == null)
                return null;

            MethodInfo m = _ParseRecordValue.MakeGenericMethod(type);

            object _this = record["this"] is IList ? ((IList)record["this"])[1] : record["this"];
            object res = m.Invoke(null, new object[] { _this, type});

            object tmp = objectPool.FirstOrDefault(p => OrmCoreTypes.AreEqual(p, res));
            if (tmp == null)
                objectPool.Add(res);
            else
                res = tmp;

            foreach (string key in record.Keys.Where(p=>p!="this"))
            {
                PropertyInfo pinfo = type.GetProperty(key);
                Type ptype = typeof(IEnumerable).IsAssignableFrom(pinfo.PropertyType) ? pinfo.PropertyType.GetGenericArguments()[0] : pinfo.PropertyType;
                if (typeof(IList).IsAssignableFrom(pinfo.PropertyType))
                {
                    List<object> lst = new List<object>();
                    foreach (IDictionary<string, object> item in ((List<object>)record[key])
                        .OrderBy(p=>((IList)((Dictionary<string,object>)p)["this"])[0]))
                    {
                        lst.Add(RecursiveMap(item, ptype, objectPool));
                    }

                    IList value = GetListProperty(pinfo, res);

                    value.Clear();
                    foreach (object item in lst)
                    {
                        value.Add(item);
                        WireKnownRelationship(pinfo, res, item);
                    }
                }
                else
                {
                    object obj = RecursiveMap(record[key] as IDictionary<string, object>, pinfo.PropertyType, objectPool);
                    pinfo.SetValue(res, obj);
                    WireKnownRelationship(pinfo, res, obj);
                }
            }
            return res;
        }
        private static IList GetListProperty(PropertyInfo pinfo, object obj)
        {
            if (!typeof(IList).IsAssignableFrom(pinfo.PropertyType))
                throw new ArgumentException($"the property type is not a collection type", nameof(pinfo));

            Type ptype = typeof(IEnumerable).IsAssignableFrom(pinfo.PropertyType) ? pinfo.PropertyType.GetGenericArguments()[0] : pinfo.PropertyType;
            IList value = pinfo.GetValue(obj) as IList;
            if (value == null)
                pinfo.SetValue(obj, value = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ptype)));

            return value;
        }
        private static void WireKnownRelationship(PropertyInfo sourceProp, object sourceObj, object destinationObj)
        {
            if (OrmCoreTypes.KnownTypeRelations.ContainsKey(sourceProp))
            {
                PropertyInfo dprop = OrmCoreTypes.KnownTypeRelations[sourceProp];

                if (typeof(IList).IsAssignableFrom(dprop.PropertyType))
                {
                    IList value = GetListProperty(dprop, destinationObj);

                    value.Add(sourceObj);
                }
                else
                {
                    dprop.SetValue(destinationObj, sourceObj);
                }
            }
        }
    }
}
