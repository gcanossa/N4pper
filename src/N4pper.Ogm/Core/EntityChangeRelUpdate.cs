using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Core
{
    public class EntityChangeRelUpdate : EntityChangeUpdate
    {
        public EntityChangeRelUpdate(IOgmConnection entity, PropertyInfo property, object oldValue, object currentValue) : base(entity, property, oldValue, currentValue)
        {
        }

        private EntityChangeDescriptor _inverse;
        public override EntityChangeDescriptor Inverse
        {
            get
            {
                if (_inverse == null)
                    _inverse = new EntityChangeRelUpdate(Entity as IOgmConnection, Property, CurrentValue, OldValue);
                return _inverse;
            }
        }
    }
}
