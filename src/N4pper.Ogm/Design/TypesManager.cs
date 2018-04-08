using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Design
{
    internal static class TypesManager
    {
        static TypesManager()
        {
            Entity<Entities.Connection>();
        }
        internal static Dictionary<Type, MethodInfo> AddNode { get; } = new Dictionary<Type, MethodInfo>();
        internal static Dictionary<Type, MethodInfo> DelNode { get; } = new Dictionary<Type, MethodInfo>();
        internal static Dictionary<Type, MethodInfo> CopyProps { get; } = new Dictionary<Type, MethodInfo>();
        internal static Dictionary<Type, KnownTypeDescriptor> KnownTypes { get; private set; } = new Dictionary<Type, KnownTypeDescriptor>();
        internal static Dictionary<Type, List<string>> KnownTypesIngnoredProperties { get; private set; } = new Dictionary<Type, List<string>>();
        internal static Dictionary<PropertyInfo, PropertyInfo> KnownTypeSourceRelations { get; private set; } = new Dictionary<PropertyInfo, PropertyInfo>();
        internal static Dictionary<PropertyInfo, PropertyInfo> KnownTypeDestinationRelations { get; private set; } = new Dictionary<PropertyInfo, PropertyInfo>();

        private static readonly MethodInfo _addNode = null;//typeof(OgmCore).GetMethods().First(p => p.Name == nameof(OgmCore.AddOrUpdateNode) && p.GetGenericArguments().Length == 1);
        private static readonly MethodInfo _delNode = null;//typeof(OgmCore).GetMethods().First(p => p.Name == nameof(OgmCore.DeleteNode));
        private static readonly MethodInfo _copyProps = typeof(ObjectExtensions).GetMethods().First(p => p.Name == nameof(ObjectExtensions.CopyProperties) && p.GetParameters()[1].ParameterType == typeof(IDictionary<string, object>));

        internal static bool AreEqual(object a, object b)
        {
            if (a == null || b == null)
                return false;
            else if (!KnownTypes.ContainsKey(a.GetType()) || !KnownTypes.ContainsKey(b.GetType()))
                return false;
            else
            {
                if (!a.GetType().IsAssignableFrom(b.GetType()) &&
                    !b.GetType().IsAssignableFrom(a.GetType()) &&
                    OMnG.TypeExtensions.Configuration.GetInterfaces(a.GetType()).Intersect(OMnG.TypeExtensions.Configuration.GetInterfaces(b.GetType())).Count() == 0)
                    return false;

                return (a as IOgmEntity)?.EntityId == (b as IOgmEntity)?.EntityId && (a as IOgmEntity)?.EntityId!=null;
            }
        }

        private static void AddType(Type type)
        {
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(type) && type.BaseType.GetGenericTypeDefinition() != typeof(Entities.ExplicitConnection<,>))
                throw new ArgumentException($"An explicit connection must inherit directly from {typeof(Entities.ExplicitConnection<,>).Name}");

            if (!KnownTypes.ContainsKey(type))
            {
                KnownTypes.Add(type, new KnownTypeDescriptor());

                //TODO: verifica
                AddNode.Add(type, _addNode.MakeGenericMethod(type));
                DelNode.Add(type, _delNode.MakeGenericMethod(type));
                CopyProps.Add(type, _copyProps.MakeGenericMethod(type));
            }
        }

        internal static void Entity<T>() where T : class, IOgmEntity
        {
            Entity(typeof(T));
        }
        internal static void Entity(Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            if (!typeof(IOgmEntity).IsAssignableFrom(type))
                throw new ArgumentException($"must be assignable to {typeof(IOgmEntity).FullName}", nameof(type));

            AddType(type);
        }

        internal static bool IsIdentityKeyNotSet<T>(this T ext) where T : class, IOgmEntity
        {
            return ext.EntityId == null;
        }
    }
}
