using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Interceptors
{
    internal class OgmCoreProxyEntityInterceptor : IInterceptor
    {
        public GraphContextBase Context { get; set; }

        public OgmCoreProxyEntityInterceptor(GraphContextBase context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public void Intercept(IInvocation invocation)
        {
            PropertyInfo pinfo = invocation.TargetType.GetProperty(invocation.Method.Name.Substring(4));
            object arg = invocation.Arguments[0];

            IOgmEntity entity;
            if (arg is IProxyTargetAccessor == false)
            {
                entity = Context.ObjectWalker.Visit(arg as IOgmEntity);
            }
            else
            {
                entity = arg as IOgmEntity;
            }

            ShadowSingle sp = null;
            if ((invocation.Proxy as IDictionary<string, object>).ContainsKey(pinfo.Name))
                sp = (invocation.Proxy as IDictionary<string, object>)[pinfo.Name] as ShadowSingle;
            if (sp == null)
                sp = new ShadowSingle(Context, pinfo, invocation.Proxy as IProxyTargetAccessor);

            sp.Set(entity);
        }
    }
}
