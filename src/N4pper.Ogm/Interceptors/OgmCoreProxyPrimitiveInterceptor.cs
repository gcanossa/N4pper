using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Interceptors
{
    internal class OgmCoreProxyPrimitiveInterceptor : IInterceptor
    {
        public GraphContextBase Context { get; set; }

        public OgmCoreProxyPrimitiveInterceptor(GraphContextBase context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public void Intercept(IInvocation invocation)
        {
            using (ManagerAccess.Manager.ScopeOMnG())
            {
                PropertyInfo pinfo = invocation.TargetType.GetProperty(invocation.Method.Name.Substring(4));
                object arg = invocation.Arguments[0];

                if (!(pinfo.ReflectedType.IsGenericType && pinfo.ReflectedType.GetGenericTypeDefinition()==typeof(Dictionary<,>)))
                {
                    if (typeof(IOgmConnection).IsAssignableFrom(invocation.TargetType))
                        Context.ChangeTracker.Track(new EntityChangeRelUpdate(invocation.InvocationTarget as IOgmConnection, pinfo, ObjectExtensions.Configuration.Get(pinfo, invocation.InvocationTarget), arg));
                    else
                        Context.ChangeTracker.Track(new EntityChangeNodeUpdate(invocation.InvocationTarget as IOgmEntity, pinfo, ObjectExtensions.Configuration.Get(pinfo, invocation.InvocationTarget), arg));
                }

                invocation.Proceed();
            }
        }
    }
}
