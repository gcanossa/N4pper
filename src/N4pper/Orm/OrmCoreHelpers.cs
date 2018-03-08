using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm
{
    internal static class OrmCoreHelpers
    {
        public static string TempSymbol(string basename = null)
        {
            basename = basename ?? "_";
            return $"{basename}{Guid.NewGuid().ToString("N")}";
        }
        public static object GetDefault(Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
