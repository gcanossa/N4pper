using N4pper.Decorators;
using N4pper.Orm.Design;
using N4pper.Orm.Entities;
using N4pper.Orm.Queryable;
using N4pper.Queryable;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Orm
{
    public abstract class GraphContext : IDisposable
    {
        public IDriver Driver { get; protected set; }
        public GraphContext(DriverProvider provider)
        {
            Driver = provider.GetDriver();
            ((OrmManagedDriver)Driver).Context = this;
            OnModelCreating(new GraphModelBuilder());
        }
        
        protected virtual void OnModelCreating(GraphModelBuilder builder)
        {
            OrmCoreTypes.Entity<Entities.Connection>(p => new { p.SourcePropertyName, p.DestinationPropertyName, p.Version });
        }

        protected List<object> ManagedObjects { get; } = new List<object>();
        protected List<object> ManagedObjectsToBeRemoved { get; } = new List<object>();

        public void Add(object obj)
        {
            if (!ManagedObjects.Contains(obj))
            {
                ManagedObjects.Add(obj);
                if(ManagedObjectsToBeRemoved.Contains(obj))
                    ManagedObjectsToBeRemoved.Remove(obj);
                object tmp = ManagedObjectsToBeRemoved.FirstOrDefault(p => OrmCoreTypes.AreEqual(p, obj));
                if (tmp != null)
                    ManagedObjectsToBeRemoved.Remove(tmp);
            }
            else
            {
                object tmp = ManagedObjects.FirstOrDefault(p => OrmCoreTypes.AreEqual(p, obj));
                if (tmp != null)
                {
                    OrmCoreTypes.CopyProps[tmp.GetType()].Invoke(null, new object[] { tmp, obj?.ToPropDictionary(), null });
                }
            }
        }
        public void Remove(object obj)
        {
            if (ManagedObjects.Contains(obj))
                ManagedObjects.Remove(obj);
            object tmp = ManagedObjects.FirstOrDefault(p => OrmCoreTypes.AreEqual(p, obj));
            if (tmp != null)
                ManagedObjects.Remove(tmp);
            if (!ManagedObjectsToBeRemoved.Contains(obj) || !ManagedObjectsToBeRemoved.Any(p => OrmCoreTypes.AreEqual(p, obj)))
                ManagedObjectsToBeRemoved.Add(obj);
        }
        public void Detach(object obj)
        {
            if (ManagedObjects.Contains(obj))
                ManagedObjects.Remove(obj);
            if (ManagedObjectsToBeRemoved.Contains(obj))
                ManagedObjectsToBeRemoved.Remove(obj);
        }

        public IQueryable<T> Query<T>(IStatementRunner runner, Action<IInclude<T>> includes = null) where T : class
        {
            OrmQueryableNeo4jStatement<T> tmp = new OrmQueryableNeo4jStatement<T>(runner, GraphContextQueryHelpers.Map);

            includes?.Invoke(tmp);

            return tmp;
        }
        
        private static readonly MethodInfo _addRel = typeof(OrmCore).GetMethods().First(p => p.Name == nameof(OrmCore.AddOrUpdateRel) && p.GetGenericArguments().Length == 3);

        private Dictionary<string, MethodInfo> AddRel { get; } = new Dictionary<string, MethodInfo>();

        private void AddOrUpdate(IStatementRunner runner)
        {
            StoringGraph graph = StoringGraph.Prepare(ManagedObjects);
            
            foreach (object obj in graph.Index.ToList())
            {
                if (!typeof(ExplicitConnection).IsAssignableFrom(obj.GetType()))
                {
                    MethodInfo m = OrmCoreTypes.AddNode[obj.GetType()];
                    MethodInfo c = OrmCoreTypes.CopyProps[obj.GetType()];
                    int i = graph.Index.IndexOf(obj);
                    graph.Index[i] = c.Invoke(null, new object[] { obj, m.Invoke(null, new object[] { runner, obj }).ToPropDictionary().SelectProperties(OrmCoreTypes.KnownTypes[obj.GetType()]), null });
                }
            }

            foreach (StoringGraph.Path path in graph.Paths
                .Where(p =>
                    !typeof(ExplicitConnection).IsAssignableFrom(TypeSystem.GetElementType(p.Property.PropertyType)) &&
                    !typeof(ExplicitConnection).IsAssignableFrom(p.Origin.GetType()))
                .Distinct(new StoringGraph.PathComparer())
                .ToList())
            {
                long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int relOrder = path.Targets.Count();
                string sourcePropName =
                         OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(path.Property) ?
                             path.Property.Name :
                             OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
                             OrmCoreTypes.KnownTypeDestinationRelations[path.Property]?.Name??"" :
                             path.Property.Name;
                string destinationPropName =
                        OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
                            path.Property.Name :
                            OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(path.Property) ?
                            OrmCoreTypes.KnownTypeSourceRelations[path.Property]?.Name??"" :
                            "";
                foreach (object obj in path.Targets.Reverse())
                {
                    string key = $"{path.Origin.GetType().FullName}:{obj.GetType().FullName}";
                    if (!AddRel.Any(p => p.Key == key))
                        AddRel.Add(key, _addRel.MakeGenericMethod(typeof(Entities.Connection), path.Origin.GetType(), obj.GetType()));
                    MethodInfo m = AddRel[key];

                    m.Invoke(null, new object[] { runner, new Entities.Connection() { SourcePropertyName = sourcePropName, DestinationPropertyName = destinationPropName, Order = relOrder--, Version = version }, path.Origin, obj });
                }
                runner.Execute(p =>
                    $"MATCH {new Node(p.Symbol(), path.Origin.GetType(), path.Origin.SelectProperties(OrmCoreTypes.KnownTypes[TypeSystem.GetElementType(path.Property.ReflectedType)]))}" +
                    $"-{p.Rel<Entities.Connection>(p.Symbol("r"), new Dictionary<string, object>() { { nameof(ExplicitConnection.SourcePropertyName), sourcePropName }, { nameof(ExplicitConnection.DestinationPropertyName), destinationPropName } })}->" +
                    $"() " +
                    $"WHERE r.Version<>$version DELETE r", new { version });
            }

            foreach (StoringGraph.Path path in graph.Paths
                .Where(p => typeof(ExplicitConnection).IsAssignableFrom(TypeSystem.GetElementType(p.Property.PropertyType)))
                .Distinct(new StoringGraph.PathComparer())
                .ToList())
            {
                long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int relOrder = path.Targets.Count();
                string sourcePropName =
                        OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(path.Property) ?
                            path.Property.Name :
                            OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
                            OrmCoreTypes.KnownTypeDestinationRelations[path.Property]?.Name :
                            path.Property.Name;
                string destinationPropName =
                        OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
                            path.Property.Name :
                            OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(path.Property) ?
                            OrmCoreTypes.KnownTypeSourceRelations[path.Property]?.Name :
                            null;
                MethodInfo c = OrmCoreTypes.CopyProps[TypeSystem.GetElementType(path.Property.PropertyType)];
                foreach (object obj in path.Targets.Reverse())
                {
                    ExplicitConnection item = obj as ExplicitConnection;
                    
                    string key = $"{item.Source.GetType().FullName}:{item.GetType().FullName}:{item.Destination.GetType().FullName}";
                    if (!AddRel.Any(p => p.Key == key))
                        AddRel.Add(key, _addRel.MakeGenericMethod(item.GetType(), item.Source.GetType(), item.Destination.GetType()));
                    MethodInfo m = AddRel[key];

                    item.Version = version;
                    item.Order = relOrder--;
                    item.SourcePropertyName = sourcePropName;

                    item.DestinationPropertyName = destinationPropName;
                    int i = graph.Index.IndexOf(obj);
                    graph.Index[i] = c.Invoke(null, new object[] { obj, m.Invoke(null, new object[] { runner, item, item.Source, item.Destination }).SelectProperties(OrmCoreTypes.KnownTypes[TypeSystem.GetElementType(path.Property.PropertyType)]), null });
                }
                runner.Execute(p =>
                    $"MATCH {new Node(p.Symbol(), path.Origin.GetType(), path.Origin.SelectProperties(OrmCoreTypes.KnownTypes[TypeSystem.GetElementType(path.Property.ReflectedType)]))}" +
                    $"-{new Rel(p.Symbol("r"), TypeSystem.GetElementType(path.Property.ReflectedType), new Dictionary<string, object>() { { nameof(ExplicitConnection.SourcePropertyName), sourcePropName }, { nameof(ExplicitConnection.DestinationPropertyName), destinationPropName } })}->" +
                    $"() " +
                    $"WHERE r.Version<>$version DELETE r", new { version });
            }
        }
        private void Delete(IStatementRunner runner)
        {
            foreach (object item in ManagedObjectsToBeRemoved)
            {
                OrmCoreTypes.DelNode[item.GetType()].Invoke(null, new object[] { runner, item });
            }
            ManagedObjectsToBeRemoved.Clear();
        }
        public void SaveChanges(IStatementRunner runner)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));

            AddOrUpdate(runner);
            Delete(runner);

            ManagedObjectsToBeRemoved.Clear();
        }

        public void Dispose()
        {
            ManagedObjects.Clear();
            ManagedObjectsToBeRemoved.Clear();
        }
    }
}
