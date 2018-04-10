using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm
{
    internal class OgmCoreProxyInterceptor : IInterceptor
    {
        public GraphContextBase Context { get; set; }

        public OgmCoreProxyInterceptor(GraphContextBase context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public void Intercept(IInvocation invocation)
        {
            if (!(invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("set_")))
            {
                invocation.Proceed();
                return;
            }

            PropertyInfo pinfo = invocation.TargetType.GetProperty(invocation.Method.Name.Substring(4));
            if (Context.TypesManager.KnownTypes.ContainsKey(invocation.TargetType) && Context.TypesManager.KnownTypes[invocation.TargetType].IgnoredProperties.Contains(pinfo))
            {
                invocation.Proceed();
                return;
            }

            object arg = invocation.Arguments[0];

            if (typeof(IOgmEntity).IsAssignableFrom(pinfo.PropertyType))
            {
                IOgmEntity entity;
                if(arg is IProxyTargetAccessor == false)
                {
                    entity = Context.ObjectWalker.Visit(arg as IOgmEntity);
                    //TODO: continua
                }
                else
                {
                    entity = arg as IOgmEntity;
                }

                //TODO: continua
            }
            else if (typeof(ICollection<IOgmEntity>).IsAssignableFrom(pinfo.PropertyType)) //TODO: usa IsGraphEntityCollection
            {
                //TODO: fai le relazioni
            }
            else
            {
                if (typeof(Connection).IsAssignableFrom(invocation.TargetType))
                    Context.ChangeTracker.Track(new EntityChangeRelUpdate(invocation.InvocationTarget as Connection, pinfo, pinfo.GetValue(invocation.InvocationTarget), arg));
                else
                    Context.ChangeTracker.Track(new EntityChangeNodeUpdate(invocation.InvocationTarget as IOgmEntity, pinfo, pinfo.GetValue(invocation.InvocationTarget), arg));

                invocation.Proceed();
                return;
            }
        }
    }
}
