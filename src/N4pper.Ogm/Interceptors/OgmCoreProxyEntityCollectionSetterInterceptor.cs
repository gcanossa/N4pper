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
    internal class OgmCoreProxyEntityCollectionSetterInterceptor : IInterceptor
    {
        public GraphContextBase Context { get; set; }

        public OgmCoreProxyEntityCollectionSetterInterceptor(GraphContextBase context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public void Intercept(IInvocation invocation)
        {
            PropertyInfo pinfo = invocation.TargetType.GetProperty(invocation.Method.Name.Substring(4));
            IEnumerable arg = invocation.Arguments[0] as IEnumerable;

            ShadowProperty sp = null;
            if ((invocation.Proxy as IDictionary<string, object>).ContainsKey(pinfo.Name))
                sp = (invocation.Proxy as IDictionary<string, object>)[pinfo.Name] as ShadowProperty;

            Type spType = typeof(ShadowCollection<>).MakeGenericType(pinfo.PropertyType.GetGenericArguments());
            if (sp == null)
            {
                sp = (ShadowProperty)Activator.CreateInstance(
                    spType,
                    Context, pinfo, invocation.Proxy as IProxyTargetAccessor);
            }

            spType.GetMethod(nameof(ICollection<int>.Clear)).Invoke(sp, new object[0]);

            foreach (object item in arg??new object[0])
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

            invocation.Proceed();
        }
    }
}
