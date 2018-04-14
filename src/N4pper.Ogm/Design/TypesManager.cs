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
            KnownTypes.Add(typeof(Entities.OgmConnection), new KnownTypeDescriptor());
            KnownTypes[typeof(Entities.OgmConnection)].IgnoredProperties.Add(typeof(Entities.IOgmConnection).GetProperty(nameof(Entities.IOgmConnection.Source)));
            KnownTypes[typeof(Entities.OgmConnection)].IgnoredProperties.Add(typeof(Entities.IOgmConnection).GetProperty(nameof(Entities.IOgmConnection.Destination)));
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
            return
                type.IsGenericType &&
                typeof(IOgmEntity).IsAssignableFrom(type.GetGenericArguments()[0]) &&
                typeof(ICollection<>).MakeGenericType(type.GetGenericArguments()).IsAssignableFrom(type);
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
            
            List<PropertyInfo> unsupported = type.GetProperties()
                .Where(
                p => 
                    (
                    !typeof(IOgmConnection).IsAssignableFrom(type) ||
                    (p.Name!=nameof(IOgmConnection.Source) && p.Name != nameof(IOgmConnection.Destination))
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
