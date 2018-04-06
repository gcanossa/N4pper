using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper
{
    public class DefaultParameterMangler : IQueryParamentersMangler
    {
        public IDictionary<string, object> Mangle(IDictionary<string, object> param)
        {
            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                if (param == null)
                    return result;

                foreach (KeyValuePair<string, object> kv in param.Where(p => p.Value == null || ObjectExtensions.IsPrimitive(p.Value.GetType())))
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
                    else
                        result.Add(kv.Key, kv.Value);
                }

                return result;
            }
        }
    }
}
