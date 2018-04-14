using Castle.DynamicProxy;
using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm
{
    internal class OgmCoreProxyInterceptorSelector : IInterceptorSelector
    {
        public GraphContextBase Context { get; set; }

        private readonly IInterceptor[] NoInterceptors = new IInterceptor[0];

        public OgmCoreProxyInterceptorSelector(GraphContextBase context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            if (Context.IsDisposed)
                throw new InvalidOperationException("The context is disposed.");

            if (IsSetter(method))
            {
                PropertyInfo pinfo = type.GetProperty(method.Name.Substring(4));
                if (!IsManagedProperty(pinfo))
                    return NoInterceptors;

                if (IsEntityCollectionProperty(pinfo))
                    return interceptors.Where(p => p is Interceptors.OgmCoreProxyEntityCollectionSetterInterceptor).ToArray();
                else if (IsEntityProperty(pinfo))
                    return interceptors.Where(p => p is Interceptors.OgmCoreProxyEntityInterceptor).ToArray();
                else
                    return interceptors.Where(p => p is Interceptors.OgmCoreProxyPrimitiveInterceptor).ToArray();
            }
            else if(IsGetter(method))
            {
                PropertyInfo pinfo = type.GetProperty(method.Name.Substring(4));
                if (!IsManagedProperty(pinfo))
                    return NoInterceptors;

                if (IsEntityCollectionProperty(pinfo))
                    return interceptors.Where(p => p is Interceptors.OgmCoreProxyEntityCollectionGetterInterceptor).ToArray();
                else
                    return NoInterceptors;
            }
            else
                return NoInterceptors;
        }

        private bool IsEntityProperty(PropertyInfo property)
        {
            return typeof(IOgmEntity).IsAssignableFrom(property.PropertyType);
        }
        private bool IsEntityCollectionProperty(PropertyInfo property)
        {
            return Context.TypesManager.IsGraphEntityCollection(property.PropertyType);
        }

        private bool IsManagedProperty(PropertyInfo property)
        {
            return !(Context.TypesManager.KnownTypes.ContainsKey(property.ReflectedType)
                && Context.TypesManager.KnownTypes[property.ReflectedType].IgnoredProperties.Contains(property));
        }

        private bool IsSetter(MethodInfo method)
        {
            return method.IsSpecialName && method.Name.StartsWith("set_", StringComparison.Ordinal);
        }
        private bool IsGetter(MethodInfo method)
        {
            return method.IsSpecialName && method.Name.StartsWith("get_", StringComparison.Ordinal);
        }
    }
}
