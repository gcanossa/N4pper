using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm
{
    internal class ObjectGraphWalker
    {
        protected TypesManager TypesManager { get; set; }
        protected ChangeTrackerBase ChangeTracker { get; set; }
        protected ProxyGenerator ProxyGenerator { get; set; }
        protected ProxyGenerationOptions ProxyGenerationOptions { get; set; }
        protected List<IInterceptor> Interceptors { get; set; }

        protected Dictionary<Type, MethodInfo> ManagedCollections { get; set; } = new Dictionary<Type, MethodInfo>();

        private void ManageCollection<T>(object obj) where T : IOgmEntity
        {
            ICollection<T> collection = (ICollection<T>)obj;
            List<IOgmEntity> old = collection.Select(p=>(IOgmEntity)p).ToList();
            collection.Clear();

            old.ForEach(p => collection.Add((T)Visit(p)));
        }

        public ObjectGraphWalker(ProxyGenerator proxyGenerator, ProxyGenerationOptions proxyGenerationOptions, TypesManager typesManager, ChangeTrackerBase changeTracker, IEnumerable<IInterceptor> interceptors)
        {
            ProxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
            ProxyGenerationOptions = proxyGenerationOptions ?? throw new ArgumentNullException(nameof(proxyGenerationOptions));
            TypesManager = typesManager ?? throw new ArgumentNullException(nameof(typesManager));
            ChangeTracker = changeTracker ?? throw new ArgumentNullException(nameof(changeTracker));

            Interceptors = interceptors?.ToList() ?? throw new ArgumentNullException(nameof(interceptors));
        }

        public IOgmEntity Visit(IOgmEntity entity)
        {
            while ((entity as IDictionary<string, object>) != null && (entity as IDictionary<string, object>)["000_ChangeTracker"] != ChangeTracker)
                entity = (entity as IProxyTargetAccessor)?.DynProxyGetTarget() as IOgmEntity;

            if (entity == null)
                return entity;
            if (entity is IProxyTargetAccessor)
                return entity;
            else
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    IOgmEntity result = (IOgmEntity)ProxyGenerator.CreateClassProxyWithTarget(entity.GetType(), entity, ProxyGenerationOptions, Interceptors.ToArray());

                    (result as IDictionary<string, object>).Add("000_ChangeTracker", ChangeTracker);

                    if (entity.EntityId==null)
                    {
                        ChangeTracker.Track(new EntityChangeNodeCreation(entity));
                    }

                    foreach (PropertyInfo pinfo in entity.GetType().GetProperties()
                        .Where(p => !ObjectExtensions.IsPrimitive(p.PropertyType) && TypesManager.IsGraphProperty(p))
                        .Where(p => !TypesManager.KnownTypes.ContainsKey(p.ReflectedType) || !TypesManager.KnownTypes[p.ReflectedType].IgnoredProperties.Contains(p)))
                    {
                        object obj = ObjectExtensions.Configuration.Get(pinfo, entity);

                        IEnumerable collection = obj as IEnumerable;
                        if (collection != null)
                        {
                            if (!ManagedCollections.ContainsKey(obj.GetType().GetGenericArguments()[0]))
                                ManagedCollections.Add(
                                    obj.GetType().GetGenericArguments()[0],
                                    GetType().GetMethod(nameof(ManageCollection), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).MakeGenericMethod(obj.GetType().GetGenericArguments()[0]));

                            ManagedCollections[obj.GetType().GetGenericArguments()[0]].Invoke(this, new[] { obj });
                        }
                        else
                        {
                            ObjectExtensions.Configuration.Set(pinfo, entity, Visit(obj as IOgmEntity));
                        }
                    }

                    return result;
                }
            }
        }
    }
}
