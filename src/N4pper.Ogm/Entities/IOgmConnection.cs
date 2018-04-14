using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Entities
{
    public interface IOgmConnection : IOgmEntity
    {
        IOgmEntity Source { get; set; }
        IOgmEntity Destination { get; set; }

        string SourcePropertyName { get; set; }
        string DestinationPropertyName { get; set; }
        int Order { get; set; }
        long Version { get; set; }
    }
    public interface IOgmConnection<S,D> : IOgmConnection 
        where S : IOgmEntity 
        where D : IOgmEntity
    {
    }
}
