using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.QueryUtils
{
    public class Set : EntityBase
    {
        public Set(Symbol symbol = null, IDictionary<string, object> props = null)
            : base(symbol, null, props)
        {
        }
        public Set SetSymbol(Symbol symbol)
        {
            Symbol = symbol;
            return this;
        }

        protected override string SerializeProps()
        {
            using (ManagerAccess.Manager.ScopeOMnG())
            {
                string s = Symbol != null ? $"{Symbol}." : "";
                return string.Join(",", Props
                    .Where(
                    p =>
                        Type == null ||
                        ObjectExtensions.IsPrimitive(Type.GetProperty(p.Key)?.PropertyType ?? typeof(IList<object>)) &&
                        (Type.GetProperty(p.Key)?.CanRead ?? false) && (Type.GetProperty(p.Key)?.CanWrite ?? false))
                    .Select(p => $"{s}{p.Key}={HandleValue(p.Value)}"));
            }
        }

        public override string Build()
        {
            return SerializeProps();
        }
    }
}
