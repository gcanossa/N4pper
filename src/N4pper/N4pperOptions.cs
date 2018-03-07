using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public class N4pperOptions
    {
        public string DefaultIdPropertyName { get; set; } = "Id";
        public Type DefaultIdPropertyType { get; set; } = typeof(long);
    }
}
