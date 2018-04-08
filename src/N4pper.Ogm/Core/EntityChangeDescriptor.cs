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
    }
}
