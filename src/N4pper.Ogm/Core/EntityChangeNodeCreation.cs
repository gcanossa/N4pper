using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using N4pper.Ogm.Entities;

namespace N4pper.Ogm.Core
{
    public class EntityChangeNodeCreation : EntityChangeDescriptor
    {
        public EntityChangeNodeCreation(IOgmEntity entity) : base(entity)
        {
            if (entity is Connection)
                throw new ArgumentException("A Connection cannot be used as node.", nameof(entity));
        }

        public override EntityChangeDescriptor Inverse => null;
    }
}
