using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using N4pper.Ogm.Interceptors;
using N4pper.Ogm.Queryable;
using N4pper.Queryable;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;

namespace N4pper.Ogm
{
    public class GraphContextBase : IGraphContext
    {
        internal TypesManager TypesManager { get; set; }
        internal ChangeTrackerBase ChangeTracker { get; set; }
        internal EntityManagerBase EntityManager { get; set; }

        internal ProxyGenerator ProxyGenerator { get; set; }

        internal List<IInterceptor> Interceptors { get; set; }
        
        internal ObjectGraphWalker ObjectWalker { get; set; }

        #region IGraphContext impl

        public IStatementRunner Runner { get; protected set; }
        public T Add<T>(T obj) where T : IOgmEntity
        {
            return (T)ObjectWalker.Visit(obj);
        }
        public void Remove(IOgmEntity obj)
        {
            if (obj is IProxyTargetAccessor)
                obj = (IOgmEntity)((IProxyTargetAccessor)obj).DynProxyGetTarget();

            if (obj is IOgmConnection)
                ChangeTracker.Track(new EntityChangeRelDeletion(obj as IOgmConnection));
            else
                ChangeTracker.Track(new EntityChangeNodeDeletion(obj));
        }
        public T Attach<T>(T obj) where T : IOgmEntity
        {
            return (T)ObjectWalker.Visit(obj);
        }
        public T Detach<T>(T obj) where T : IOgmEntity
        {
            if (obj is IProxyTargetAccessor)
            {
                IOgmEntity result = (IOgmEntity)((IProxyTargetAccessor)obj).DynProxyGetTarget();

                ChangeTracker.Untrack(result);

                return (T)result;
            }
            else
                return obj;
        }
        public void SaveChanges()
        {
            try
            {
                IEnumerable<EntityChangeDescriptor> temp = ChangeTracker.GetChangeLog();

                List<Tuple<IOgmEntity, IEnumerable<string>>> creatingNodes = temp
                    .Where(p => p is EntityChangeNodeCreation)
                    .Select(p => new Tuple<IOgmEntity, IEnumerable<string>>(
                        p.Entity,
                        TypesManager.KnownTypes.ContainsKey(p.Entity.GetType()) ? TypesManager.KnownTypes[p.Entity.GetType()].IgnoredProperties.Select(q => q.Name) : null
                    )).ToList();

                List<IOgmEntity> deletingNodes = temp
                    .Where(p => p is EntityChangeNodeDeletion)
                    .Select(p => p.Entity).ToList();

                List<Tuple<IOgmEntity, IEnumerable<string>>> updatingNodes = temp
                    .Where(p => p is EntityChangeNodeUpdate)
                    .GroupBy(p => p.Entity)
                    .Select(p => new Tuple<IOgmEntity, IEnumerable<string>>(
                        p.Key,
                        p.Key.GetType().GetProperties().Select(q => q.Name).Except(p.Select(q => ((EntityChangeNodeUpdate)q).Property.Name))
                        .Union(
                        TypesManager.KnownTypes.ContainsKey(p.Key.GetType()) ? TypesManager.KnownTypes[p.Key.GetType()].IgnoredProperties.Select(q => q.Name) : new string[0]
                        )
                    )).ToList();

                List<Tuple<IOgmConnection, IEnumerable<string>>> creatingRels = temp
                    .Where(p => p is EntityChangeRelCreation)
                    .Select(p => new Tuple<IOgmConnection, IEnumerable<string>>(
                        p.Entity as IOgmConnection,
                        TypesManager.KnownTypes.ContainsKey(p.Entity.GetType()) ? TypesManager.KnownTypes[p.Entity.GetType()].IgnoredProperties.Select(q => q.Name) : null
                    )).ToList();

                List<IOgmConnection> deletingRels = temp
                    .Where(p => p is EntityChangeRelDeletion)
                    .Select(p => p.Entity as IOgmConnection).ToList();

                List<Tuple<IOgmConnection, IEnumerable<string>>> updatingRels = temp
                    .Where(p => p is EntityChangeRelUpdate)
                    .GroupBy(p => p.Entity)
                    .Select(p => new Tuple<IOgmConnection, IEnumerable<string>>(
                        p.Key as IOgmConnection,
                        p.Key.GetType().GetProperties().Select(q => q.Name).Except(p.Select(q => ((EntityChangeRelUpdate)q).Property.Name))
                        .Union(
                        TypesManager.KnownTypes.ContainsKey(p.Key.GetType()) ? TypesManager.KnownTypes[p.Key.GetType()].IgnoredProperties.Select(q => q.Name) : new string[0]
                        )
                    )).ToList();

                List<Tuple<IOgmConnection, IEnumerable<string>>> mergingRels = temp
                    .Where(p => p is EntityChangeConnectionMerge)
                    .Select(p => new Tuple<IOgmConnection, IEnumerable<string>>(
                        p.Entity as IOgmConnection,
                        TypesManager.KnownTypes.ContainsKey(p.Entity.GetType()) ? TypesManager.KnownTypes[p.Entity.GetType()].IgnoredProperties.Select(q => q.Name) : null
                    )).ToList();

                List<IOgmEntity> createdNodes = EntityManager.CreateNodes(Runner, creatingNodes)?.ToList();
                EntityManager.UpdateNodes(Runner, updatingNodes);
                EntityManager.DeleteNodes(Runner, deletingNodes);

                for (int i = 0; i < (createdNodes?.Count??0); i++)
                {
                    creatingNodes[i].Item1.EntityId = createdNodes[i].EntityId;
                }

                List<IOgmConnection> createdRels = EntityManager.CreateRels(Runner, creatingRels)?.ToList();

                List<IOgmConnection> mergedRels = EntityManager.MergeConnections(Runner, mergingRels)?.ToList();

                EntityManager.UpdateRels(Runner, updatingRels);
                EntityManager.DeleteRels(Runner, deletingRels);

                for (int i = 0; i < (createdRels?.Count ?? 0); i++)
                {
                    creatingRels[i].Item1.EntityId = createdRels[i].EntityId;
                }
                for (int i = 0; i < (mergedRels?.Count ?? 0); i++)
                {
                    mergingRels[i].Item1.EntityId = mergedRels[i].EntityId;
                }
                
                ChangeTracker.Clear();
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        public IQueryable<T> Query<T>(Action<IInclude<T>> includes = null) where T : class, IOgmEntity
        {
            return null;
            //TODO: ole ola
            //if (typeof(ExplicitConnection).IsAssignableFrom(typeof(T)))
            //    throw new InvalidOperationException("Quering explicit connections directly is not allowed.");

            //OgmQueryableNeo4jStatement<T> tmp = new OgmQueryableNeo4jStatement<T>(Runner, (r, t) => GraphContextQueryHelpers.Map(r, t, ManagedObjects));

            //includes?.Invoke(tmp);

            //return tmp;
        }
        #endregion

        internal GraphContextBase(TypesManager typesManager, ChangeTrackerBase changeTracker, EntityManagerBase entityManager)
        {
            TypesManager = typesManager ?? throw new ArgumentNullException(nameof(typesManager));
            ChangeTracker = changeTracker ?? throw new ArgumentNullException(nameof(changeTracker));
            EntityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));

            Interceptors = new List<IInterceptor>();
            Interceptors.Add(new OgmCoreProxyPrimitiveInterceptor(this));
            Interceptors.Add(new OgmCoreProxyEntityInterceptor(this));
            Interceptors.Add(new OgmCoreProxyEntityCollectionSetterInterceptor(this));
            Interceptors.Add(new OgmCoreProxyEntityCollectionGetterInterceptor(this));

            ProxyGenerationOptions options = new ProxyGenerationOptions() { Selector = new OgmCoreProxyInterceptorSelector(this) };
            options.AddMixinInstance(new Dictionary<string, object>());
            ProxyGenerator = new ProxyGenerator();

            ObjectWalker = new ObjectGraphWalker(ProxyGenerator, options, typesManager,changeTracker, Interceptors);
        }

        public GraphContextBase(IStatementRunner runner, TypesManager typesManager, ChangeTrackerBase changeTracker, EntityManagerBase entityManager)
            :this(typesManager, changeTracker,entityManager)
        {
            Runner = runner ?? throw new ArgumentNullException(nameof(runner));
        }

        #region internals


        //private void AddOrUpdate()
        //{
        //    StoringGraph graph = StoringGraph.Prepare(ManagedObjects);

        //    foreach (object obj in graph.Index.ToList())
        //    {
        //        if (!typeof(ExplicitConnection).IsAssignableFrom(obj.GetType()))
        //        {
        //            MethodInfo m = TypesManager.AddNode[obj.GetType()];
        //            MethodInfo c = TypesManager.CopyProps[obj.GetType()];
        //            int i = graph.Index.IndexOf(obj);
        //            //TODO: verifica
        //            //graph.Index[i] = c.Invoke(null, new object[] { obj, m.Invoke(null, new object[] { Runner, obj }).ToPropDictionary().SelectProperties(TypesManager.KnownTypes[obj.GetType()]), null });
        //        }
        //    }

        //    foreach (StoringGraph.Path path in graph.Paths
        //        .Where(p =>
        //            !typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType)) &&
        //            !typeof(ExplicitConnection).IsAssignableFrom(p.Origin.GetType()))
        //        .Distinct(new StoringGraph.PathComparer())
        //        .ToList())
        //    {
        //        long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        //        int relOrder = path.Targets.Count();
        //        string sourcePropName =
        //                 TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property) ?
        //                     path.Property.Name :
        //                     TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
        //                     TypesManager.KnownTypeDestinationRelations[path.Property]?.Name ?? "" :
        //                     path.Property.Name;
        //        string destinationPropName =
        //                TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
        //                    path.Property.Name :
        //                    TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property) ?
        //                    TypesManager.KnownTypeSourceRelations[path.Property]?.Name ?? "" :
        //                    "";
        //        foreach (object obj in path.Targets.Reverse())
        //        {
        //            string key = $"{path.Origin.GetType().FullName}:{obj.GetType().FullName}";
        //            if (!AddRel.Any(p => p.Key == key))
        //                AddRel.Add(key, _addRel.MakeGenericMethod(typeof(Entities.Connection), path.Origin.GetType(), obj.GetType()));
        //            MethodInfo m = AddRel[key];

        //            m.Invoke(null, new object[] { Runner, new Entities.Connection() { SourcePropertyName = sourcePropName, DestinationPropertyName = destinationPropName, Order = relOrder--, Version = version }, path.Origin, obj });
        //        }
        //        Runner.Execute(p =>//TODO: verifica
        //            //$"MATCH {new Node(p.Symbol(), path.Origin.GetType(), path.Origin.SelectProperties(TypesManager.KnownTypes[ObjectExtensions.GetElementType(path.Property.ReflectedType)]))}" +
        //            $"-{p.Rel<Entities.Connection>(p.Symbol("r"), new Dictionary<string, object>() { { nameof(ExplicitConnection.SourcePropertyName), sourcePropName }, { nameof(ExplicitConnection.DestinationPropertyName), destinationPropName } })}->" +
        //            $"() " +
        //            $"WHERE r.Version<>$version DELETE r", new { version });
        //    }

        //    foreach (StoringGraph.Path path in graph.Paths
        //        .Where(p => typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType)))
        //        .Distinct(new StoringGraph.PathComparer())
        //        .ToList())
        //    {
        //        long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        //        int relOrder = path.Targets.Count();
        //        string sourcePropName =
        //                TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property) ?
        //                    path.Property.Name :
        //                    TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
        //                    TypesManager.KnownTypeDestinationRelations[path.Property]?.Name :
        //                    path.Property.Name;
        //        string destinationPropName =
        //                TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
        //                    path.Property.Name :
        //                    TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property) ?
        //                    TypesManager.KnownTypeSourceRelations[path.Property]?.Name :
        //                    null;
        //        MethodInfo c = TypesManager.CopyProps[ObjectExtensions.GetElementType(path.Property.PropertyType)];
        //        foreach (object obj in path.Targets.Reverse())
        //        {
        //            ExplicitConnection item = obj as ExplicitConnection;

        //            string key = $"{item.Source.GetType().FullName}:{item.GetType().FullName}:{item.Destination.GetType().FullName}";
        //            if (!AddRel.Any(p => p.Key == key))
        //                AddRel.Add(key, _addRel.MakeGenericMethod(item.GetType(), item.Source.GetType(), item.Destination.GetType()));
        //            MethodInfo m = AddRel[key];

        //            item.Version = version;
        //            item.Order = relOrder--;
        //            item.SourcePropertyName = sourcePropName;

        //            item.DestinationPropertyName = destinationPropName;
        //            int i = graph.Index.IndexOf(obj);
        //            //TODO: verifica
        //            //graph.Index[i] = c.Invoke(null, new object[] { obj, m.Invoke(null, new object[] { Runner, item, item.Source, item.Destination }).SelectProperties(TypesManager.KnownTypes[ObjectExtensions.GetElementType(path.Property.PropertyType)]), null });
        //        }
        //        Runner.Execute(p =>//TODO: verifica
        //            //$"MATCH {new Node(p.Symbol(), path.Origin.GetType(), path.Origin.SelectProperties(TypesManager.KnownTypes[ObjectExtensions.GetElementType(path.Property.ReflectedType)]))}" +
        //            $"-{new Rel(p.Symbol("r"), ObjectExtensions.GetElementType(path.Property.ReflectedType), new Dictionary<string, object>() { { nameof(ExplicitConnection.SourcePropertyName), sourcePropName }, { nameof(ExplicitConnection.DestinationPropertyName), destinationPropName } })}->" +
        //            $"() " +
        //            $"WHERE r.Version<>$version DELETE r", new { version });
        //    }
        //}

        #endregion

        internal bool IsDisposed { get; private set; } = false;
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                ChangeTracker.Clear();
                ChangeTracker = null;
                TypesManager = null;
                EntityManager = null;
                Interceptors.Clear();
                Interceptors = null;
                ObjectWalker = null;
                ProxyGenerator = null;

                Runner.Dispose();
            }
        }
    }
}
