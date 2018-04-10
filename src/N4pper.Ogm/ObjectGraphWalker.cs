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

namespace N4pper.Ogm
{
    internal class ObjectGraphWalker
    {
        protected TypesManager TypesManager { get; set; }
        protected ChangeTrackerBase ChangeTracker { get; set; }
        protected ProxyGenerator ProxyGenerator { get; set; }
        protected List<IInterceptor> Interceptors { get; set; }

        public ObjectGraphWalker(ProxyGenerator proxyGenerator, TypesManager typesManager, ChangeTrackerBase changeTracker, IEnumerable<IInterceptor> interceptors)
        {
            ProxyGenerator = proxyGenerator ?? throw new ArgumentNullException(nameof(proxyGenerator));
            TypesManager = typesManager ?? throw new ArgumentNullException(nameof(typesManager));
            ChangeTracker = changeTracker ?? throw new ArgumentNullException(nameof(changeTracker));

            Interceptors = interceptors?.ToList() ?? throw new ArgumentNullException(nameof(interceptors));
        }

        public IOgmEntity Visit(IOgmEntity entity)
        {
            if (entity == null)
                return entity;
            if (entity is IProxyTargetAccessor)
                return entity;
            else
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    ProxyGenerationOptions options = new ProxyGenerationOptions();
                    options.AddMixinInstance(new Dictionary<string, object>());
                    IOgmEntity result = (IOgmEntity)ProxyGenerator.CreateClassProxyWithTarget(entity.GetType(), entity, options, Interceptors.ToArray());

                    if(entity.EntityId==null)
                    {
                        ChangeTracker.Track(new EntityChangeNodeCreation(entity));
                    }

                    //TODO: invece di ICollection<IOgmEntity> usa IsGraphEntityCollection
                    foreach (PropertyInfo pinfo in entity.GetType().GetProperties()
                        .Where(p => typeof(IOgmEntity).IsAssignableFrom(p.PropertyType) || typeof(ICollection<IOgmEntity>).IsAssignableFrom(p.PropertyType))
                        .Where(p => !TypesManager.KnownTypes.ContainsKey(p.ReflectedType) || !TypesManager.KnownTypes[p.ReflectedType].IgnoredProperties.Contains(p)))
                    {
                        object obj = ObjectExtensions.Configuration.Get(pinfo, entity);

                        ICollection<IOgmEntity> collection = obj as ICollection<IOgmEntity>;
                        if (collection != null)
                        {
                            List<IOgmEntity> old = collection.ToList();
                            collection.Clear();
                            old.ForEach(p=>collection.Add(Visit(p)));
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
