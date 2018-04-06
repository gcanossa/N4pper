﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using N4pper.Orm.Design;
using N4pper.Orm.Entities;
using N4pper.Orm.Queryable;
using N4pper.Queryable;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;

namespace N4pper.Orm
{
    public class GraphContextBase : IGraphContext
    {
        #region IGraphContext impl

        public IStatementRunner Runner { get; protected set; }
        public void Add(object obj)
        {
            if (!ManagedObjects.Contains(obj))
            {
                ManagedObjects.Add(obj);
                if (ManagedObjectsToBeRemoved.Contains(obj))
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
        public void SaveChanges()
        {
            AddOrUpdate();
            Delete();

            ManagedObjectsToBeRemoved.Clear();
        }
        public IQueryable<T> Query<T>(Action<IInclude<T>> includes = null) where T : class
        {
            if (typeof(ExplicitConnection).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException("Quering explicit connections directly is not allowed.");

            OrmQueryableNeo4jStatement<T> tmp = new OrmQueryableNeo4jStatement<T>(Runner, (r, t) => GraphContextQueryHelpers.Map(r, t, ManagedObjects));

            includes?.Invoke(tmp);

            return tmp;
        }
        #endregion

        internal GraphContextBase()
        {

        }
        public GraphContextBase(IStatementRunner runner)
        {
            Runner = runner ?? throw new ArgumentNullException(nameof(runner));
        }

        #region internals

        private List<object> ManagedObjects { get; } = new List<object>();
        private List<object> ManagedObjectsToBeRemoved { get; } = new List<object>();

        private static readonly MethodInfo _addRel = typeof(OrmCore).GetMethods().First(p => p.Name == nameof(OrmCore.AddOrUpdateRel) && p.GetGenericArguments().Length == 3);

        private Dictionary<string, MethodInfo> AddRel { get; } = new Dictionary<string, MethodInfo>();

        private void AddOrUpdate()
        {
            StoringGraph graph = StoringGraph.Prepare(ManagedObjects);

            foreach (object obj in graph.Index.ToList())
            {
                if (!typeof(ExplicitConnection).IsAssignableFrom(obj.GetType()))
                {
                    MethodInfo m = OrmCoreTypes.AddNode[obj.GetType()];
                    MethodInfo c = OrmCoreTypes.CopyProps[obj.GetType()];
                    int i = graph.Index.IndexOf(obj);
                    graph.Index[i] = c.Invoke(null, new object[] { obj, m.Invoke(null, new object[] { Runner, obj }).ToPropDictionary().SelectProperties(OrmCoreTypes.KnownTypes[obj.GetType()]), null });
                }
            }

            foreach (StoringGraph.Path path in graph.Paths
                .Where(p =>
                    !typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType)) &&
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
                             OrmCoreTypes.KnownTypeDestinationRelations[path.Property]?.Name ?? "" :
                             path.Property.Name;
                string destinationPropName =
                        OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(path.Property) ?
                            path.Property.Name :
                            OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(path.Property) ?
                            OrmCoreTypes.KnownTypeSourceRelations[path.Property]?.Name ?? "" :
                            "";
                foreach (object obj in path.Targets.Reverse())
                {
                    string key = $"{path.Origin.GetType().FullName}:{obj.GetType().FullName}";
                    if (!AddRel.Any(p => p.Key == key))
                        AddRel.Add(key, _addRel.MakeGenericMethod(typeof(Entities.Connection), path.Origin.GetType(), obj.GetType()));
                    MethodInfo m = AddRel[key];

                    m.Invoke(null, new object[] { Runner, new Entities.Connection() { SourcePropertyName = sourcePropName, DestinationPropertyName = destinationPropName, Order = relOrder--, Version = version }, path.Origin, obj });
                }
                Runner.Execute(p =>
                    $"MATCH {new Node(p.Symbol(), path.Origin.GetType(), path.Origin.SelectProperties(OrmCoreTypes.KnownTypes[ObjectExtensions.GetElementType(path.Property.ReflectedType)]))}" +
                    $"-{p.Rel<Entities.Connection>(p.Symbol("r"), new Dictionary<string, object>() { { nameof(ExplicitConnection.SourcePropertyName), sourcePropName }, { nameof(ExplicitConnection.DestinationPropertyName), destinationPropName } })}->" +
                    $"() " +
                    $"WHERE r.Version<>$version DELETE r", new { version });
            }

            foreach (StoringGraph.Path path in graph.Paths
                .Where(p => typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType)))
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
                MethodInfo c = OrmCoreTypes.CopyProps[ObjectExtensions.GetElementType(path.Property.PropertyType)];
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
                    graph.Index[i] = c.Invoke(null, new object[] { obj, m.Invoke(null, new object[] { Runner, item, item.Source, item.Destination }).SelectProperties(OrmCoreTypes.KnownTypes[ObjectExtensions.GetElementType(path.Property.PropertyType)]), null });
                }
                Runner.Execute(p =>
                    $"MATCH {new Node(p.Symbol(), path.Origin.GetType(), path.Origin.SelectProperties(OrmCoreTypes.KnownTypes[ObjectExtensions.GetElementType(path.Property.ReflectedType)]))}" +
                    $"-{new Rel(p.Symbol("r"), ObjectExtensions.GetElementType(path.Property.ReflectedType), new Dictionary<string, object>() { { nameof(ExplicitConnection.SourcePropertyName), sourcePropName }, { nameof(ExplicitConnection.DestinationPropertyName), destinationPropName } })}->" +
                    $"() " +
                    $"WHERE r.Version<>$version DELETE r", new { version });
            }
        }
        private void Delete()
        {
            foreach (object item in ManagedObjectsToBeRemoved)
            {
                OrmCoreTypes.DelNode[item.GetType()].Invoke(null, new object[] { Runner, item });
            }
            ManagedObjectsToBeRemoved.Clear();
        }

        #endregion

        public void Dispose()
        {
            ManagedObjects.Clear();
            ManagedObjectsToBeRemoved.Clear();
            Runner.Dispose();
        }
    }
}
