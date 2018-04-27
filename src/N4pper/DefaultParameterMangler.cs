using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper
{
    public class DefaultParameterMangler : IQueryParamentersMangler
    {
        private bool IsPrimitiveCollection(Type type)
        {
            return type?.GetGenericArgumentsOf(typeof(ICollection<>))?.FirstOrDefault()?.FirstOrDefault()?.Type?.IsPrimitive()??false;
        }

        protected object MangleValue(object value)
        {
            if (value == null)
                return null;
            else if (value.IsDateTime())
            {
                DateTimeOffset d = value is DateTimeOffset ? (DateTimeOffset)value : (DateTime)value;
                return d.ToUnixTimeMilliseconds();
            }
            else if (value.IsTimeSpan())
                return ((TimeSpan)value).TotalMilliseconds;
            else if (value.GetType() == typeof(Guid) || value.GetType() == typeof(Guid?))
                return value.ToString();
            else if (value.IsPrimitive())
                return value;
            else if (value.GetType().IsEnum)
                return (int)value;
            else if (IsPrimitiveCollection(value.GetType()))
            {
                List<object> lst = new List<object>();
                foreach (object item in value as IEnumerable)
                {
                    lst.Add(MangleValue(item));
                }
                return lst;
            }
            else if (value.GetType().IsEnumerable() && !value.GetType().IsOfGenericType(typeof(IDictionary<,>)))
            {
                List<object> lst = new List<object>();
                foreach (object item in value as IEnumerable)
                {
                    lst.Add(MangleObject(item));
                }
                return lst;
            }
            else
                return MangleObject(value);
        }
        protected IDictionary<string, object> MangleObject(object param)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (param == null)
                return result;

            foreach (KeyValuePair<string, object> kv in 
                (param is IDictionary<string, object> ? (IDictionary<string, object>)param : param.ToPropDictionary()))
            {
                result.Add(kv.Key, MangleValue(kv.Value));
            }

            return result;
        }
        public IDictionary<string, object> Mangle(object param)
        {
            using (ManagerAccess.Manager.ScopeOMnG())
            {
                return MangleObject(param) as IDictionary<string, object>;
            }
        }
    }
}
