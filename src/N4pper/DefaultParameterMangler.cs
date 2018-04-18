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
        protected IDictionary<string, object> MangleImpl(object param)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (param == null)
                return result;

            foreach (KeyValuePair<string, object> kv in (param is IDictionary<string, object> ? (IDictionary<string, object>)param : param.ToPropDictionary()))
            {
                if (kv.Value == null)
                    result.Add(kv.Key, kv.Value);
                else if (kv.Value.IsDateTime())
                {
                    DateTimeOffset d = kv.Value is DateTimeOffset ? (DateTimeOffset)kv.Value : (DateTime)kv.Value;
                    result.Add(kv.Key, d.ToUnixTimeMilliseconds());
                }
                else if (kv.Value.IsTimeSpan())
                    result.Add(kv.Key, ((TimeSpan)kv.Value).TotalMilliseconds);
                else if (kv.Value.IsPrimitive())
                    result.Add(kv.Key, kv.Value);
                else if (kv.Value is IEnumerable)
                {
                    List<IDictionary<string, object>> lst = new List<IDictionary<string, object>>();
                    foreach (object item in kv.Value as IEnumerable)
                    {
                        lst.Add(MangleImpl(item));
                    }
                    result.Add(kv.Key, lst);
                }
                else
                    result.Add(kv.Key, MangleImpl(kv.Value));
            }

            return result;
        }
        public IDictionary<string, object> Mangle(object param)
        {
            using (ManagerAccess.Manager.ScopeOMnG())
            {
                return MangleImpl(param);
            }
        }
    }
}
