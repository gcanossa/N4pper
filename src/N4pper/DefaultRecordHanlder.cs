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

                if (assigningType.IsEnumerable())
                {                 
                    return ParseRecordsValue((IList<object>)value, assigningType.GetGenericArgumentsOf(typeof(IEnumerable<>)).First()[0].Type, realType.GetGenericArgumentsOf(typeof(IEnumerable<>)).First()[0].Type);
                }
                else
                {
                    if (value is IList<object>)
                        return ParseRecordsValue((IList<object>)value, assigningType, realType);
                    else if (value is IEntity && assigningType.IsAssignableFrom(realType))
                        return MapEntity(assigningType, (IEntity)value);
                    else
                        return realType.GetInstanceOf(GetPropDictionary(value));
                }
            }
        }
        private IList ParseRecordsValue(IList<object> value, Type assigningType, Type realType)
        {
            IList lst = (IList)typeof(List<>).MakeGenericType(realType).GetInstanceOf(null);
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
            Type tp = obj.GetType().GetProperty(p.Key)?.PropertyType;
            Type innerTp = tp?.GetGenericArgumentsOf(typeof(ICollection<>))?.FirstOrDefault()?.FirstOrDefault()?.Type;

            if (innerTp != null
                && innerTp.IsPrimitive())
            {
                Type listType = tp.IsInterface || tp.IsAbstract ? typeof(List<>).MakeGenericType(innerTp) : tp;
                object val = (IList)Activator.CreateInstance(listType);
                MethodInfo m = tp.GetMethod("Add");
                foreach (object item in (IEnumerable)p.Value)
                {
                    m.Invoke(val, new[] { item?.ConvertTo(listType.GetGenericArguments()[0]) });
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

            return obj.CopyProperties(((IDictionary<string, object>)entity.Properties.ToDictionary(p => p.Key, p => 
            {
                object tmp = TryWrapList(obj, p);

                if (tmp != null)
                    return tmp;
                else
                    return p.Value;
            })).SelectProperties(obj.GetType().GetProperties().Select(p=>p.Name)));
        }
    }
}
