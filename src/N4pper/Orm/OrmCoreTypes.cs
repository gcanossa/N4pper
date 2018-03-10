using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace N4pper.Orm
{
    public static class OrmCoreTypes
    {
        internal static TypeExtensionsConfiguration OMnGConfiguration { get; } = new TypeExtensionsConfiguration.CompressConfiguration();
        internal static Dictionary<Type, IEnumerable<string>> KnownTypes { get; private set; } = new Dictionary<Type, IEnumerable<string>>();

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
            if (KnownTypes.ContainsKey(type))
                throw new InvalidOperationException($"type {type.FullName} already managed");

            KnownTypes.Add(type, keyProps);
        }

        public static void Entity<T>(Expression<Func<T,object>> expr) where T : class
        {
            Entity<T>(ObjectExtensions.ToPropertyNameCollection(expr));
        }
        public static void Entity<T>(IEnumerable<string> keyProps = null) where T : class
        {
            Entity(typeof(T), keyProps);
        }
        public static void Entity(Type type, IEnumerable<string> keyProps = null)
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
                .Any(p => p.Value == null || p.Value == OrmCoreHelpers.GetDefault(p.Value.GetType())))
                throw new ArgumentException($"Every key property, except for '{Constants.IdentityPropertyName}', must have a vaue different from the default(T)");
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
            return value == null || value.Equals(OrmCoreHelpers.GetDefault(info.PropertyType));
        }
    }
}
