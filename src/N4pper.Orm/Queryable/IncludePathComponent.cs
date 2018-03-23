using N4pper.QueryUtils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace N4pper.Orm.Queryable
{
    internal class IncludePathComponent
    {
        public PropertyInfo Property { get; set; }
        public bool IsEnumerable { get; set; }
        public Symbol Symbol { get; set; }
    }
}
