using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm.Entities
{
    public sealed class Connection
    {
        public string PropertyName { get; set; }
        public int Order { get; set; }
        public long Version { get; set; }
    }
}
