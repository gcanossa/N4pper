using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Entities
{
    public class Connection : IOgmEntity
    {
        public virtual string SourcePropertyName { get; set; }
        public virtual string DestinationPropertyName { get; set; }
        public virtual int Order { get; set; }
        public virtual long Version { get; set; }
        public virtual long? EntityId { get; set; }
    }
}
