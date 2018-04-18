using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper
{
    public class DefaultRecordHanlder : IRecordHandler
    {
        public object ParseRecordValue(object value, Type assigningType, Type realType)
        {
            using (ManagerAccess.Manager.ScopeOMnG())
            {
                assigningType = assigningType ?? throw new ArgumentNullException(nameof(assigningType));
                realType = realType ?? throw new ArgumentNullException(nameof(realType));

                if (ObjectExtensions.IsCollection(assigningType))
                {
                    if (!ObjectExtensions.IsEnumerable(assigningType))
                        throw new InvalidOperationException("To map collections use IEnumerable`1");
                    
                    return ParseRecordsValue((IList<object>)value, assigningType.GetGenericArguments()[0], realType.GetGenericArguments()[0]);
                }
                else
                {
                    if (value is IList<object>)
                        return ParseRecordsValue((IList<object>)value, assigningType, realType);
                    if (value is IEntity && assigningType.IsAssignableFrom(realType))
                        return MapEntity(assigningType, (IEntity)value);
                    else
                        return ObjectExtensions.GetInstanceOf(realType, GetPropDictionary(value));
                }
            }
        }
        private IList ParseRecordsValue(IList<object> value, Type assigningType, Type realType)
        {
            IList lst = ObjectExtensions.GetListOf(realType);
            foreach (object item in value.Select(p => ParseRecordValue(p, assigningType, realType)))
            {
                lst.Add(item);
            }
            return lst;
        }
        private IDictionary<string, object> GetPropDictionary(object entity)
        {
            if (entity is IEntity)
                return ((IEntity)entity).Properties.ToDictionary(p => p.Key, p => p.Value);
            else if (entity is IDictionary<string, object>)
                return ((IDictionary<string, object>)entity);
            else
                return null;
        }

        private object TryWrapList(object obj, KeyValuePair<string, object> p)
        {
            Type tp = obj.GetType().GetProperty(p.Key).PropertyType;
            Type innerTp = tp.GetInterface("ICollection`1")?.GetGenericArguments()?.First();
            if (innerTp == null && tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(ICollection<>))
                innerTp = tp.GetGenericArguments()?.First();

            if (innerTp != null
                && ObjectExtensions.IsPrimitive(innerTp))
            {
                object val = (IList)Activator.CreateInstance(tp.IsInterface || tp.IsAbstract ? typeof(List<>).MakeGenericType(innerTp) : tp);
                MethodInfo m = tp.GetMethod("Add");
                foreach (object item in (IEnumerable)p.Value)
                {
                    m.Invoke(val, new[] { item });
                }
                return val;
            }
            else
                return null;
        }

        private object MapEntity(Type assigningType, IEntity entity)
        {
            assigningType = assigningType ?? throw new ArgumentNullException(nameof(assigningType));

            object obj = null;
            if (entity is INode)
            {
                obj = ((INode)entity).Labels.GetTypesFromLabels().GetInstanceOfMostSpecific();
            }
            else if (entity is IRelationship)
            {
                obj = new string[] { ((IRelationship)entity).Type }.GetTypesFromLabels().GetInstanceOfMostSpecific();
            }

            return obj.CopyProperties(entity.Properties.ToDictionary(p => p.Key, p => 
            {
                object tmp = TryWrapList(obj, p);

                if (tmp != null)
                    return tmp;
                else
                    return p.Value;
            }));
        }
    }
}
