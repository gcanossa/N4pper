using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Core
{
    public class EntityChangeRelCreation : EntityChangeDescriptor
    {
        public EntityChangeRelCreation(IOgmEntity entity) : base(entity)
        {
        }

        public override EntityChangeDescriptor Inverse => null;
    }
}
