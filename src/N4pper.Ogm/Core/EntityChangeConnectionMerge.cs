using System;
using System.Collections.Generic;
using System.Text;
using N4pper.Ogm.Entities;

namespace N4pper.Ogm.Core
{
    public class EntityChangeConnectionMerge : EntityChangeDescriptor
    {
        public IOgmEntity Source { get; set; }
        public IOgmEntity Destination { get; set; }
        public int Order { get; set; }
        public EntityChangeConnectionMerge(IOgmConnection entity, IOgmEntity source, IOgmEntity destination, int order) : base(entity)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));

            if (source is IOgmConnection)
                throw new ArgumentException("A Connection cannot be a source of a connction.", nameof(source));
            if (destination is IOgmConnection)
                throw new ArgumentException("A Connection cannot be a destination of a connction.", nameof(destination));
            Order = order;
        }

        public override EntityChangeDescriptor Inverse => null;

        public override bool Equals(object obj)
        {
            EntityChangeConnectionMerge other = obj as EntityChangeConnectionMerge;
            return base.Equals(obj) && other?.Source == Source && other?.Destination == Destination;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Source.GetHashCode() ^ Destination.GetHashCode();
        }
    }
}
