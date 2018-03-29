using N4pper.QueryUtils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace N4pper.Orm.Queryable
{
    internal class IncludePathComponent
    {
        public PropertyInfo SourceProperty { get; set; }
        public PropertyInfo DestinationProperty { get; set; }
        public bool IsEnumerable { get; set; }
        public bool IsReverse { get; set; }
        public Symbol Symbol { get; set; }
        public Symbol RelSymbol { get; set; }
        public string Label { get; set; }
    }
}
