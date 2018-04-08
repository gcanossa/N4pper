using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using N4pper.Ogm.Entities;

namespace N4pper.Ogm.Core
{
    public class EntityChangeNodeUpdate : EntityChangeUpdate
    {
        public EntityChangeNodeUpdate(IOgmEntity entity, PropertyInfo property, object oldValue, object currentValue) : base(entity, property, oldValue, currentValue)
        {
        }

        private EntityChangeDescriptor _inverse;
        public override EntityChangeDescriptor Inverse
        {
            get
            {
                if (_inverse == null)
                    _inverse = new EntityChangeNodeUpdate(Entity, Property, CurrentValue, OldValue);
                return _inverse;
            }
        }
    }
}
