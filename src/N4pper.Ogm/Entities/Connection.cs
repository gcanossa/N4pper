using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Entities
{
    public sealed class Connection : IOgmEntity
    {
        public string SourcePropertyName { get; set; }
        public string DestinationPropertyName { get; set; }
        public int Order { get; set; }
        public long Version { get; set; }
        public long? EntityId { get; set; }
    }
}
