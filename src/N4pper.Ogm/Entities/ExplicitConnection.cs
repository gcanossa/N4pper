using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Entities
{
    public abstract class ExplicitConnection : IOgmEntity
    {
        public virtual object Source { get; set; }
        public virtual object Destination { get; set; }

        public virtual string SourcePropertyName { get; internal set; }
        public virtual string DestinationPropertyName { get; internal set; }
        public virtual int Order { get; internal set; }
        public virtual long Version { get; internal set; }
        public long? EntityId { get; set; }
    }
    public abstract class ExplicitConnection<S, D> : ExplicitConnection
        where S : class, IOgmEntity
        where D : class, IOgmEntity
    {
        public new S Source { get { return (S)base.Source; } set { base.Source = (S)value; } }
        public new D Destination { get { return (D)base.Destination; } set { base.Destination = (D)value; } }
    }
}
