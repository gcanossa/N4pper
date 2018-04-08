using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Core
{
    public abstract class EntityChangeUpdate : EntityChangeDescriptor
    {
        public PropertyInfo Property { get; protected set; }
        public object OldValue { get; protected set; }
        public object CurrentValue { get; protected set; }
        public EntityChangeUpdate(IOgmEntity entity, PropertyInfo property, object oldValue, object currentValue) : base(entity)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));

            if (IsCompatibleWith(property, oldValue))
                throw new ArgumentException("The value must be compatible with the property type", nameof(oldValue));
            if (IsCompatibleWith(property, currentValue))
                throw new ArgumentException("The value must be compatible with the property type", nameof(currentValue));

            OldValue = oldValue;
            CurrentValue = currentValue;
        }

        protected bool IsCompatibleWith(PropertyInfo property, object value)
        {
            return property.PropertyType.IsValueType && value == null || property.PropertyType.IsAssignableFrom(value.GetType());
        }
    }
}
