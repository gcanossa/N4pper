using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Design
{
    public class TypesManager
    {
        public TypesManager()
        {
            Entity<Entities.Connection>();
        }
        
        public IDictionary<Type, KnownTypeDescriptor> KnownTypes { get; protected set; } = new Dictionary<Type, KnownTypeDescriptor>();
        public IDictionary<PropertyInfo, PropertyInfo> KnownTypeSourceRelations { get; private set; } = new Dictionary<PropertyInfo, PropertyInfo>();
        public IDictionary<PropertyInfo, PropertyInfo> KnownTypeDestinationRelations { get; private set; } = new Dictionary<PropertyInfo, PropertyInfo>();
        
        public bool IsGraphProperty(PropertyInfo property)
        {
            return !(property.GetAccessors().Any(q => !q.IsVirtual) ||
                            (
                                !ObjectExtensions.IsPrimitive(property.PropertyType) &&
                                !typeof(IOgmEntity).IsAssignableFrom(property.PropertyType) &&
                                !IsGraphEntityCollection(property.PropertyType)
                            ));
        }
        public bool IsGraphEntityCollection(Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type) &&
                type.IsGenericType &&
                typeof(IOgmEntity).IsAssignableFrom(type.GetGenericArguments()[0]);
        }

        public void Entity<T>(bool ignoreUnsupported = false) where T : class, IOgmEntity
        {
            Entity(typeof(T), ignoreUnsupported);
        }
        public void Entity(Type type, bool ignoreUnsupported = false)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            if (!typeof(IOgmEntity).IsAssignableFrom(type))
                throw new ArgumentException($"must be assignable to {typeof(IOgmEntity).FullName}", nameof(type));

            if (type.IsSealed)
                throw new ArgumentException("Unable to manage sealed types.", nameof(type));
            if (type.GetMethods().Where(p=>p.Name != nameof(Object.GetType) && !p.IsSpecialName).Any(p => !p.IsVirtual))
                throw new ArgumentException("Unable to manage type with non virtual methods",nameof(type));

            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(type) && type.BaseType.GetGenericTypeDefinition() != typeof(Entities.ExplicitConnection<,>))
                throw new ArgumentException($"An explicit connection must inherit directly from {typeof(Entities.ExplicitConnection<,>).Name}");

            List<PropertyInfo> unsupported = type.GetProperties()
                .Where(
                p => 
                    (
                    !typeof(Entities.ExplicitConnection).IsAssignableFrom(type) ||
                    (p.Name!=nameof(ExplicitConnection.Source) && p.Name != nameof(ExplicitConnection.Destination))
                    ) && !IsGraphProperty(p)
                ).ToList();
            if (unsupported.Count > 0 && !ignoreUnsupported)
                throw new ArgumentException($"Unable to manage type with non virtual properties or properties no deriving from {typeof(IOgmEntity).FullName} or compatible with {typeof(ICollection<IOgmEntity>).FullName}. Set '{nameof(ignoreUnsupported)}' parameter in order to ignore them.");
            
            if (!KnownTypes.ContainsKey(type))
            {
                KnownTypes.Add(type, new KnownTypeDescriptor());
            }
            
            KnownTypes[type].IgnoredProperties.AddRange(unsupported);
        }
    }
}
