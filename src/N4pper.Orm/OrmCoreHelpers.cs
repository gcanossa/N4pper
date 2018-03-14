using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm
{
    internal static class OrmCoreHelpers
    {
        internal static object GetDefault(Type type)
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
