using System;
using System.Collections.Generic;
using System.Text;
using N4pper.Ogm.Entities;

namespace N4pper.Ogm.Core
{
    public class EntityChangeRelDeletion : EntityChangeDescriptor
    {
        public EntityChangeRelDeletion(Connection entity) : base(entity)
        {
        }

        public override EntityChangeDescriptor Inverse => null;
    }
}
