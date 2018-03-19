using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.QueryUtils
{
    public class Set : EntityBase
    {
        public Set(Symbol symbol = null, Dictionary<string, object> props = null)
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
            string s = Symbol != null ? $"{Symbol}." : "";
            return string.Join(",", Props.Select(p => $"{s}{p.Key}={HandleValue(p.Value)}"));
        }

        public override string Build()
        {
            return SerializeProps();
        }
    }
}
