using N4pper.Orm.Design;
using N4pper.Queryable;
using Neo4j.Driver.V1;
using OMnG;
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
        public static object Map(IRecord record, Type type, List<object> pool)
        {
            return RecursiveMap(record[1] as IDictionary<string, object>, type, pool);
        }

        private static object GetObject(IDictionary<string, object> record, Type type, List<object> objectPool)
        {
            MethodInfo m = _ParseRecordValue.MakeGenericMethod(type);

            object _this = record["this"] is IList ? ((IList)record["this"])[1] : record["this"];

            object res = m.Invoke(null, new object[] { _this, type });

            object tmp = objectPool.FirstOrDefault(p => OrmCoreTypes.AreEqual(p, res));
            if (tmp == null)
                objectPool.Add(res);
            else
                res = tmp;

            return res;
        }

        private static object RecursiveMap(IDictionary<string, object> record, Type type, List<object> objectPool)
        {
            if (record == null)
                return null;
            
            object res = GetObject(record, type, objectPool);

            foreach (string key in record.Keys.Where(p=>p!="this"))
            {
                PropertyInfo pinfo = type.GetProperty(key);
                Type ptype = TypeSystem.GetElementType(pinfo.PropertyType);
                if (typeof(IList).IsAssignableFrom(pinfo.PropertyType))
                {
                    IEnumerable<Tuple<IRelationship, IDictionary<string, object>>> coll = ((List<object>)record[key])
                        .Select(p => (IDictionary<string, object>)p)
                        .Select(p => new Tuple<IRelationship, IDictionary<string, object>>
                        (
                            (IRelationship)((IList)p["this"])[0],
                            p
                        ))
                        .Where(p=>p.Item1!=null);

                    if (coll.Count() == 0)
                        continue;

                    List<object> lst = new List<object>();
                    Type relType = new string[] { coll.First().Item1.Type }.GetTypesFromLabels().First();
                    if (typeof(Entities.ExplicitConnection).IsAssignableFrom(relType))
                    {
                        foreach (Tuple<IRelationship, IDictionary<string, object>> item in coll
                            .OrderBy(p => p.Item1["Order"]))
                        {
                            Entities.ExplicitConnection rel = (Entities.ExplicitConnection)GetObject(new Dictionary<string, object>() { { "this", item.Item1 } }, relType, objectPool);
                            lst.Add(rel);
                            
                            if (item.Item1.EndNodeId == ((INode)((IList)item.Item2["this"])[1]).Id)
                            {
                                rel.Destination = RecursiveMap(item.Item2, rel.GetType().BaseType.GetGenericArguments()[1], objectPool);
                            }
                            else
                            {
                                rel.Source = RecursiveMap(item.Item2, rel.GetType().BaseType.GetGenericArguments()[0], objectPool);
                            }
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
                        foreach (Tuple<IRelationship, IDictionary<string, object>> item in coll
                            .OrderBy(p => p.Item1["Order"]))
                        {
                            lst.Add(RecursiveMap(item.Item2, ptype, objectPool));
                        }

                        IList value = GetListProperty(pinfo, res);

                        value.Clear();
                        foreach (object item in lst)
                        {
                            value.Add(item);
                            WireKnownRelationship(pinfo, res, item);
                        }
                    }
                }
                else
                {
                    IDictionary<string, object> rec = record[key] as IDictionary<string, object>;

                    if(rec["this"] is IList)
                    {
                        Type relType = new string[] { ((IRelationship)(((IList)rec["this"])[0])).Type }.GetTypesFromLabels().First();

                        Entities.ExplicitConnection rel = (Entities.ExplicitConnection)GetObject(new Dictionary<string, object>() { { "this", ((IList)rec["this"])[0] } }, relType, objectPool);
                        
                        if (((IRelationship)(((IList)rec["this"])[0])).EndNodeId == ((INode)(((IList)rec["this"])[1])).Id)
                        {
                            rel.Destination = RecursiveMap(rec as IDictionary<string, object>, rel.GetType().BaseType.GetGenericArguments()[1], objectPool);
                        }
                        else
                        {
                            rel.Source = RecursiveMap(rec as IDictionary<string, object>, rel.GetType().BaseType.GetGenericArguments()[0], objectPool);
                        }

                        pinfo.SetValue(res, rel);
                        WireKnownRelationship(pinfo, res, rel);
                    }
                    else
                    {
                        object obj = RecursiveMap(rec, pinfo.PropertyType, objectPool);
                        pinfo.SetValue(res, obj);
                        WireKnownRelationship(pinfo, res, obj);
                    }
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
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(destinationObj.GetType()))
            {
                Entities.ExplicitConnection ec = destinationObj as Entities.ExplicitConnection;
                ec.Source = ec.Source ?? sourceObj;
                ec.Destination = ec.Destination ?? sourceObj;
            }
            else
            {
                if (OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(sourceProp) && OrmCoreTypes.KnownTypeSourceRelations[sourceProp] != null)
                {
                    PropertyInfo dprop = OrmCoreTypes.KnownTypeSourceRelations[sourceProp];

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
                if (OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(sourceProp) && OrmCoreTypes.KnownTypeDestinationRelations[sourceProp] != null)
                {
                    PropertyInfo dprop = OrmCoreTypes.KnownTypeDestinationRelations[sourceProp];

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
}
