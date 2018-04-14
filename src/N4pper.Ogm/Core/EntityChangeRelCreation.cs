using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Core
{
    public class EntityChangeRelCreation : EntityChangeDescriptor
    {
        public IOgmEntity Source { get; set; }
        public IOgmEntity Destination { get; set; }
        public EntityChangeRelCreation(IOgmConnection entity, IOgmEntity source, IOgmEntity destination) : base(entity)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));

            if (source is IOgmConnection)
                throw new ArgumentException("A Connection cannot be a source of a connction.", nameof(source));
            if (destination is IOgmConnection)
                throw new ArgumentException("A Connection cannot be a destination of a connction.", nameof(destination));
        }

        public override EntityChangeDescriptor Inverse => null;

        public override bool Equals(object obj)
        {
            EntityChangeRelCreation other = obj as EntityChangeRelCreation;
            return base.Equals(obj) && other?.Source == Source && other?.Destination == Destination;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Source.GetHashCode() ^ Destination.GetHashCode();
        }
    }
}
