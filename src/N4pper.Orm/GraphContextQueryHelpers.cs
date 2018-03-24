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
            return RecursiveMap(record[1] as IDictionary<string, object>, type);
        }
        private static object RecursiveMap(IDictionary<string, object> record, Type type)
        {
            //TODO: resuse existing objects don't create duplicates
            if (record == null)
                return null;

            MethodInfo m = _ParseRecordValue.MakeGenericMethod(type);

            object res = m.Invoke(null, new object[] { record["this"], type});
            foreach (string key in record.Keys.Where(p=>p!="this"))
            {
                PropertyInfo pinfo = type.GetProperty(key);
                Type ptype = typeof(IEnumerable).IsAssignableFrom(pinfo.PropertyType) ? pinfo.PropertyType.GetGenericArguments()[0] : pinfo.PropertyType;
                if (typeof(IList).IsAssignableFrom(pinfo.PropertyType))
                {
                    List<object> lst = new List<object>();
                    foreach (IDictionary<string, object> item in (IList)record[key])
                    {
                        lst.Add(RecursiveMap(item, ptype));
                    }

                    IList value = pinfo.GetValue(res) as IList;
                    if (value == null)
                        pinfo.SetValue(res, value = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ptype)));

                    value.Clear();
                    foreach (object item in lst)
                    {
                        value.Add(item);
                    }
                }
                else
                {
                    pinfo.SetValue(res, RecursiveMap(record[key] as IDictionary<string, object>, pinfo.PropertyType));
                }
            }
            return res;
        }
    }
}
