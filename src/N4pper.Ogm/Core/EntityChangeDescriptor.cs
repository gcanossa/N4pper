using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Core
{
    public abstract class EntityChangeDescriptor
    {
        public IOgmEntity Entity { get; protected set; }
        public EntityChangeDescriptor(IOgmEntity entity)
        {
            Entity = entity;
        }

        public abstract EntityChangeDescriptor Inverse { get; }

        public override bool Equals(object obj)
        {
            return obj != null && GetType().Equals(obj?.GetType()) && ((obj as EntityChangeDescriptor)?.Entity?.Equals(Entity)??false);
        }
        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Entity.GetHashCode();
        }
    }
}
