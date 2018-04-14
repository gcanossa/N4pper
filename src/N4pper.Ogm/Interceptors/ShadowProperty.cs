using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Interceptors
{
    public abstract class ShadowProperty
    {
        public GraphContextBase Context { get; set; }
        public PropertyInfo Property { get; set; }
        public IProxyTargetAccessor ProxyAccessor { get; set; }

        public ShadowProperty(GraphContextBase context, PropertyInfo property, IProxyTargetAccessor proxyAccessor)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            ProxyAccessor = proxyAccessor ?? throw new ArgumentNullException(nameof(proxyAccessor));

            Attach();
        }

        protected virtual void Attach()
        {
            if ((ProxyAccessor as IDictionary<string, object>).ContainsKey(Property.Name))
                (ProxyAccessor as IDictionary<string, object>).Add(Property.Name, this);
            else
                (ProxyAccessor as IDictionary<string, object>)[Property.Name] = this;
        }

        protected bool IsInverse
        {
            get
            {
                return Property == DestinationProperty;
            }
        }
        protected PropertyInfo SourceProperty
        {
            get
            {
                if (Context.TypesManager.KnownTypeSourceRelations.ContainsKey(Property))
                {
                    return Property;
                }
                else if (Context.TypesManager.KnownTypeDestinationRelations.ContainsKey(Property))
                {
                    return Context.TypesManager.KnownTypeDestinationRelations[Property];
                }
                else
                {
                    return Property;
                }
            }
        }
        protected PropertyInfo DestinationProperty
        {
            get
            {
                if (Context.TypesManager.KnownTypeSourceRelations.ContainsKey(Property))
                {
                    return Context.TypesManager.KnownTypeSourceRelations[Property];
                }
                else if (Context.TypesManager.KnownTypeDestinationRelations.ContainsKey(Property))
                {
                    return Property;
                }
                else
                {
                    return null;
                }
            }
        }

        protected void Unwire(IOgmConnection connection)
        {
            if (connection != null)
            {
                if (IsInverse)
                {
                    if (SourceProperty != null)
                    {
                        using (ManagerAccess.Manager.ScopeOMnG())
                        {
                            if (Context.TypesManager.IsGraphEntityCollection(SourceProperty.PropertyType))
                            {
                                object coll = ObjectExtensions.Configuration.Get(SourceProperty, connection.Source);
                                if (coll != null)
                                    coll.GetType().GetMethod(nameof(ICollection<int>.Remove)).Invoke(coll, new[] { connection });
                            }
                            else
                            {
                                ObjectExtensions.Configuration.Set(SourceProperty, connection.Source, null);
                            }
                        }
                    }
                }
                else
                {
                    if (DestinationProperty != null)
                    {
                        using (ManagerAccess.Manager.ScopeOMnG())
                        {
                            if (Context.TypesManager.IsGraphEntityCollection(DestinationProperty.PropertyType))
                            {
                                object coll = ObjectExtensions.Configuration.Get(DestinationProperty, connection.Destination);
                                if (coll != null)
                                    coll.GetType().GetMethod(nameof(ICollection<int>.Remove)).Invoke(coll, new[] { connection });
                            }
                            else
                            {
                                ObjectExtensions.Configuration.Set(DestinationProperty,connection.Destination, null);
                            }
                        }
                    }
                }
            }
        }
        protected void Wire(IOgmConnection connection)
        {
            if (connection != null)
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    if (IsInverse)
                    {
                        if (Context.TypesManager.IsGraphEntityCollection(DestinationProperty.PropertyType))
                        {
                            object coll = ObjectExtensions.Configuration.Get(DestinationProperty,connection.Destination);
                            coll.GetType().GetMethod(nameof(ICollection<int>.Add)).Invoke(coll, new[] { connection is OgmConnection ? connection.Destination : connection });
                        }
                        else
                            ObjectExtensions.Configuration.Set(DestinationProperty,ProxyAccessor.DynProxyGetTarget(), connection is OgmConnection ? connection.Destination : connection);

                        if (SourceProperty != null)
                        {
                            if (Context.TypesManager.IsGraphEntityCollection(SourceProperty.PropertyType))
                            {
                                object coll = ObjectExtensions.Configuration.Get(SourceProperty,connection.Source);
                                if (coll == null)
                                {
                                    if (SourceProperty.CanWrite)
                                    {
                                        coll = Activator.CreateInstance(SourceProperty.PropertyType);
                                        ObjectExtensions.Configuration.Set(SourceProperty,connection.Source, coll);
                                    }
                                    else
                                        throw new InvalidOperationException($"Unable to set property {SourceProperty.ReflectedType.FullName}.{SourceProperty.Name}");
                                }
                                coll.GetType().GetMethod(nameof(ICollection<int>.Add)).Invoke(coll, new[] { connection.Destination });
                            }
                            else
                            {
                                ObjectExtensions.Configuration.Set(SourceProperty,connection.Source, connection.Destination);
                            }
                        }
                    }
                    else
                    {
                        if (Context.TypesManager.IsGraphEntityCollection(SourceProperty.PropertyType))
                        {
                            object coll = ObjectExtensions.Configuration.Get(SourceProperty,connection.Source);
                            coll.GetType().GetMethod(nameof(ICollection<int>.Add)).Invoke(coll, new[] { connection is OgmConnection ? connection.Destination : connection });
                        }
                        else
                            ObjectExtensions.Configuration.Set(SourceProperty,ProxyAccessor.DynProxyGetTarget(), connection is OgmConnection ? connection.Destination : connection);

                        if (DestinationProperty != null)
                        {
                            if (Context.TypesManager.IsGraphEntityCollection(DestinationProperty.PropertyType))
                            {
                                object coll = ObjectExtensions.Configuration.Get(DestinationProperty,connection.Destination);
                                if (coll == null)
                                {
                                    if (DestinationProperty.CanWrite)
                                    {
                                        coll = Activator.CreateInstance(DestinationProperty.PropertyType);
                                        ObjectExtensions.Configuration.Set(DestinationProperty,connection.Destination, coll);
                                    }
                                    else
                                        throw new InvalidOperationException($"Unable to set property {DestinationProperty.ReflectedType.FullName}.{DestinationProperty.Name}");
                                }
                                coll.GetType().GetMethod(nameof(ICollection<int>.Add)).Invoke(coll, new[] { connection.Source });
                            }
                            else
                            {
                                ObjectExtensions.Configuration.Set(DestinationProperty,connection.Destination, connection.Source);
                            }
                        }
                    }
                }
            }
        }
    }
}
