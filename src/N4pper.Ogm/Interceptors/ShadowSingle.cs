using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Entities;

namespace N4pper.Ogm.Interceptors
{
    public class ShadowSingle : ShadowProperty
    {
        public ShadowSingle(GraphContextBase context, PropertyInfo property, IProxyTargetAccessor proxyAccessor) : base(context, property, proxyAccessor)
        {
        }

        public IOgmConnection Connection { get; set; }

        protected void DiscardEffect()
        {
            if(Connection!=null)
                Context.ChangeTracker.Track(new EntityChangeRelDeletion(Connection));
        }


        public void Set(IOgmEntity entity)
        {
            if (entity is IOgmConnection && !IsInverse && (entity as IOgmConnection).Destination == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Destination)}' property must be set.");
            if (entity is IOgmConnection && IsInverse && (entity as IOgmConnection).Source == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Source)}' property must be set.");

            DiscardEffect();

            long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            IOgmConnection tmp = Connection;
            Connection = null;
            Unwire(tmp);

            Connection = (entity as IOgmConnection) ?? new OgmConnection() { Order = 0, SourcePropertyName = SourceProperty?.Name??"", DestinationPropertyName = DestinationProperty?.Name??"", Version = version  };

            if(Connection is OgmConnection)
            {
                Connection.Source = IsInverse ? entity : ProxyAccessor as IOgmEntity;
                Connection.Destination = IsInverse ? ProxyAccessor as IOgmEntity : entity;
            }

            Wire(Connection);
        }
    }
}
