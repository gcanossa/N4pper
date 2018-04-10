using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Entities
{
    public abstract class ExplicitConnection : Connection
    {
        public virtual IOgmEntity Source { get; set; }
        public virtual IOgmEntity Destination { get; set; }
    }
    public abstract class ExplicitConnection<S, D> : ExplicitConnection
        where S : class, IOgmEntity
        where D : class, IOgmEntity
    {
        public new S Source { get { return (S)base.Source; } set { base.Source = (S)value; } }
        public new D Destination { get { return (D)base.Destination; } set { base.Destination = (D)value; } }
    }
}
