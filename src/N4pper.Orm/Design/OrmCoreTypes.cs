using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace N4pper.Orm.Design
{
    internal static class OrmCoreTypes
    {
        internal static Dictionary<Type, MethodInfo> AddNode { get; } = new Dictionary<Type, MethodInfo>();
        internal static Dictionary<Type, MethodInfo> DelNode { get; } = new Dictionary<Type, MethodInfo>();
        internal static Dictionary<Type, MethodInfo> CopyProps { get; } = new Dictionary<Type, MethodInfo>();
        internal static Dictionary<Type, IEnumerable<string>> KnownTypes { get; private set; } = new Dictionary<Type, IEnumerable<string>>();
        internal static Dictionary<Type, List<string>> KnownTypesIngnoredProperties { get; private set; } = new Dictionary<Type, List<string>>();
        internal static Dictionary<PropertyInfo, PropertyInfo> KnownTypeSourceRelations { get; private set; } = new Dictionary<PropertyInfo, PropertyInfo>();
        internal static Dictionary<PropertyInfo, PropertyInfo> KnownTypeDestinationRelations { get; private set; } = new Dictionary<PropertyInfo, PropertyInfo>();

        private static readonly MethodInfo _addNode = typeof(OrmCore).GetMethods().First(p => p.Name == nameof(OrmCore.AddOrUpdateNode) && p.GetGenericArguments().Length == 1);
        private static readonly MethodInfo _delNode = typeof(OrmCore).GetMethods().First(p => p.Name == nameof(OrmCore.DeleteNode));
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
                    a.GetType().GetInterfaces().Intersect(b.GetType().GetInterfaces()).Count() == 0)
                    return false;

                IDictionary<string, object> _a = a.SelectProperties(KnownTypes[a.GetType()]);
                IDictionary<string, object> _b = b.SelectProperties(KnownTypes[b.GetType()]);

                if (_a.Count != _b.Count)
                    return false;
                else
                {
                    foreach (KeyValuePair<string, object> kv in _a)
                    {
                        if (kv.Value==null || _b[kv.Key]==null || !kv.Value.Equals(_b[kv.Key]))
                            return false;
                    }
                    return true;
                }
            }
        }

        private static void ValidateKey(Type type, IEnumerable<string> keyProps)
        {
            foreach (string k in keyProps)
            {
                PropertyInfo info = type.GetProperty(k);
                if (info == null)
                    throw new ArgumentException($"key proerty '{k}' not found for type {type.FullName}");
                if (!ObjectExtensions.IsPrimitive(info.PropertyType))
                    throw new ArgumentException($"key proerty '{k}' must be a primitive type");
            }
        }
        private static void AddType(Type type, IEnumerable<string> keyProps)
        {
            if (!KnownTypes.ContainsKey(type))
            {
                KnownTypes.Add(type, keyProps);
                KnownTypesIngnoredProperties.Add(type, new List<string>());

                AddNode.Add(type, _addNode.MakeGenericMethod(type));
                DelNode.Add(type, _delNode.MakeGenericMethod(type));
                CopyProps.Add(type, _copyProps.MakeGenericMethod(type));
            }
            else if (string.Join(",", KnownTypes[type].OrderBy(p => p)) != string.Join(",", keyProps.OrderBy(p => p)))
                throw new InvalidOperationException($"type {type.FullName} already managed with a different key: '{string.Join(",", KnownTypes[type].OrderBy(p => p))}'");
        }

        internal static void Entity<T>(Expression<Func<T,object>> expr) where T : class
        {
            Entity<T>(ObjectExtensions.ToPropertyNameCollection(expr));
        }
        internal static void Entity<T>(IEnumerable<string> keyProps = null) where T : class
        {
            Entity(typeof(T), keyProps);
        }
        internal static void Entity(Type type, IEnumerable<string> keyProps = null)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));

            if (keyProps == null || keyProps.Count() == 0)
                keyProps = new[] { Constants.IdentityPropertyName };

            ValidateKey(type, keyProps);
            AddType(type, keyProps);
        }

        internal static void ValidateObjectKeyValues<T>(this T ext) where T : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            if (ext
                .SelectProperties(KnownTypes[typeof(T)])
                .ExludeProperties(new[] { Constants.IdentityPropertyName })
                .Any(p => p.Value == null || p.Value == ObjectExtensions.GetDefault(p.Value.GetType())))
                throw new ArgumentException($"Every key property, except for '{Constants.IdentityPropertyName}', must have a value different from the default(T)");
        }
        internal static bool HasIdentityKey<T>()
        {
            return KnownTypes[typeof(T)].Contains(Constants.IdentityPropertyName);
        }
        internal static bool HasIdentityKey<T>(this T ext)
        {
            return HasIdentityKey<T>();
        }
        internal static bool IsIdentityKeyNotSet<T>(this T ext)
        {
            PropertyInfo info = typeof(T).GetProperty(Constants.IdentityPropertyName);
            object value = info.GetValue(ext);
            return value == null || value.Equals(ObjectExtensions.GetDefault(info.PropertyType));
        }
    }
}
