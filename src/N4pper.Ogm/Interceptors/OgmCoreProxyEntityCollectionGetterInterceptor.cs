using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Interceptors
{
    internal class OgmCoreProxyEntityCollectionGetterInterceptor : IInterceptor
    {
        public GraphContextBase Context { get; set; }

        public OgmCoreProxyEntityCollectionGetterInterceptor(GraphContextBase context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public void Intercept(IInvocation invocation)
        {
            PropertyInfo pinfo = invocation.TargetType.GetProperty(invocation.Method.Name.Substring(4));

            ShadowProperty sp = null;
            if ((invocation.Proxy as IDictionary<string, object>).ContainsKey(pinfo.Name))
                sp = (invocation.Proxy as IDictionary<string, object>)[pinfo.Name] as ShadowProperty;
            if (sp == null)
            {
                Type spType = typeof(ShadowCollection<>).MakeGenericType(pinfo.PropertyType.GetGenericArguments());
                sp = (ShadowProperty)Activator.CreateInstance(
                    spType,
                    Context, pinfo, invocation.Proxy as IProxyTargetAccessor);

                foreach (object item in (invocation.Method.Invoke(invocation.InvocationTarget, new object[0]) as IEnumerable)??new object[0])
                {
                    IOgmEntity entity;
                    if (item is IProxyTargetAccessor == false)
                    {
                        entity = Context.ObjectWalker.Visit(item as IOgmEntity);
                    }
                    else
                    {
                        entity = item as IOgmEntity;
                    }

                    spType.GetMethod(nameof(ICollection<int>.Add)).Invoke(sp, new[] { entity });
                }
            }

            object target = pinfo.GetValue(invocation.InvocationTarget);
            if (target == null && pinfo.CanWrite)
                pinfo.SetValue(invocation.InvocationTarget, Activator.CreateInstance(pinfo.PropertyType));

            invocation.ReturnValue = Context.ProxyGenerator.CreateInterfaceProxyWithTarget(
                pinfo.PropertyType,
                pinfo.PropertyType.GetInterfaces(),
                target,
                (IInterceptor)Activator.CreateInstance(typeof(CollectionInterceptor<>).MakeGenericType(pinfo.PropertyType.GetGenericArguments()),sp));
        }

        public class CollectionInterceptor<T> : IInterceptor where T : class, IOgmEntity
        {
            public ShadowCollection<T> Property { get; set; }
            public CollectionInterceptor(ShadowCollection<T> property)
            {
                Property = property;
            }
            public void Intercept(IInvocation invocation)
            {
                if (invocation.Method.DeclaringType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    invocation.ReturnValue = invocation.Method.Invoke(Property, invocation.Arguments);
                    //invocation.ReturnValue = typeof(ShadowCollection).GetMethod(invocation.Method.Name).Invoke(Property, invocation.Arguments);
                }
                else
                    invocation.Proceed();
            }
        }
    }
}
