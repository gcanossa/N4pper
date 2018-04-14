using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Entities
{
    internal class OgmConnection : IOgmConnection
    {
        public virtual IOgmEntity Source { get; set; }
        public virtual IOgmEntity Destination { get; set; }
        public virtual string SourcePropertyName { get; set; }
        public virtual string DestinationPropertyName { get; set; }
        public virtual int Order { get; set; }
        public virtual long Version { get; set; }
        public virtual long? EntityId { get; set; }
    }
}
