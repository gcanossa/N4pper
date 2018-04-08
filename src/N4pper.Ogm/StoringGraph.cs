using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using N4pper.Queryable;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm
{
    internal class StoringGraph
    {
        #region nested types
        public class PathComparer : IEqualityComparer<Path>
        {
            public bool Equals(Path x, Path y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(Path obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }
        public class Path
        {
            private StoringGraph _graph;
            public PropertyInfo Property { get; private set; }
            private int _origin;
            private List<int> _targets = new List<int>();

            public object Origin => _graph.Index[_origin];
            public IEnumerable<object> Targets => _targets.Select(p=>_graph.Index[p]);

            public Path(StoringGraph graph, PropertyInfo pinfo, object origin, IEnumerable<object> targets)
            {
                origin = origin ?? throw new ArgumentNullException(nameof(origin));

                _graph = graph;
                Property = pinfo;
                _origin = graph.Index.IndexOf(origin);
                if (_origin < 0)
                    throw new ArgumentException("Unable to find the origin in the index");
                _targets = targets?.Select(p=> graph.Index.IndexOf(p))?.ToList()??new List<int>();
                if(_targets.Any(p=>p<0))
                    throw new ArgumentException("Unable to find a target in the index");
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                else if (obj.GetType() != typeof(Path))
                    return false;
                else
                    return ((Path)obj).Property == Property && ((Path)obj)._origin == _origin && ((Path)obj)._targets.Intersect(_targets).Count() == _targets.Count;
            }
            public override int GetHashCode()
            {
                return Property.GetHashCode() ^ Origin.GetHashCode() ^ Targets.Aggregate<object, int, int>(0,(a,p)=>a^p.GetHashCode(), p=>p);
            }
        }
        #endregion
        public List<object> Index { get; } = new List<object>();
        public HashSet<Path> Paths { get; } = new HashSet<Path>();
        
        public static StoringGraph Prepare(IEnumerable<object> objects)
        {
            StoringGraph graph = new StoringGraph();

            if (objects != null)
            {
                foreach (object item in objects)
                {
                    TraverseForPrepare(null, item, graph);
                }
            }

            FixGraphReferences(graph);

            PruneEquivalentPaths(graph);

            return graph;
        }
        private static void TraverseForPrepare(object parentObj, object obj, StoringGraph graph)
        {
            if (graph.Index.Contains(obj))
                return;
            else
            {
                graph.Index.Add(obj);
                
                foreach (PropertyInfo pInfo in obj.GetType().GetProperties()
                    .Where(p => !ObjectExtensions.IsPrimitive(p.PropertyType) && ObjectExtensions.Configuration.Get(p,obj) != null))
                {
                    Type ienumerable = ObjectExtensions.FindIEnumerable(pInfo.PropertyType);
                    if (ienumerable != null && !ObjectExtensions.IsPrimitive(ObjectExtensions.GetElementType(ienumerable)))
                    {
                        object tmp = ObjectExtensions.Configuration.Get(pInfo,obj);
                        List<object> targets = new List<object>();
                        foreach (object value in ((IEnumerable)tmp))
                        {
                            TraverseForPrepare(obj, value, graph);
                        }
                        foreach (object value in ((IEnumerable)tmp))
                        {
                            targets.Add(value);
                        }
                        graph.Paths.Add(new Path(graph, pInfo, obj, targets));
                    }
                    else
                    {
                        object value = ObjectExtensions.Configuration.Get(pInfo,obj);
                        TraverseForPrepare(obj, value, graph);
                        graph.Paths.Add(new Path(graph, pInfo, obj, new[] { value }));
                    }
                }
            }
        }
        private static void FixGraphReferences(StoringGraph graph)
        {
            foreach (Path path in graph.Paths.Where(p => typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType))).ToList())
            {
                if (
                    TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property) &&
                    !TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property))
                {
                    FixExplicitConnectionSource(graph, path);
                }
                else if (
                    !TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property) &&
                    TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property))
                {
                    FixExplicitConnectionDestination(graph, path);
                }
                else if (
                    TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property) &&
                    TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property))
                {
                    FixExplicitConnectionSame(graph, path);
                }
                else //default is always only source with no destination return
                {
                    FixExplicitConnectionSource(graph, path);
                }
            }
            foreach (Path path in graph.Paths.Where(p => typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType))).ToList())
                CheckExplicitConnection(graph, path);

            foreach (Path path in graph.Paths
                .Where(p => 
                    !typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType)) &&
                    !typeof(ExplicitConnection).IsAssignableFrom(p.Origin.GetType())))
            {
                if (TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property))
                {
                    FixImplicitConnectionSource(graph, path);
                }
                else if (TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property))
                {
                    FixImplicitConnectionDestination(graph, path);
                }
                else //default is always only source with no destination return
                {
                    
                }
            }
            foreach (Path path in graph.Paths
                .Where(p =>
                    !typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType)) &&
                    !typeof(ExplicitConnection).IsAssignableFrom(p.Origin.GetType())))
                CheckImplicitConnection(graph, path);
        }
        private static void PruneEquivalentPaths(StoringGraph graph)
        {
            IEnumerable<Path> notExplicitDsts = graph.Paths.Where(p =>
                    !typeof(ExplicitConnection).IsAssignableFrom(ObjectExtensions.GetElementType(p.Property.PropertyType)) &&
                    !typeof(ExplicitConnection).IsAssignableFrom(p.Origin.GetType()) &&
                    TypesManager.KnownTypeDestinationRelations.ContainsKey(p.Property) &&
                    TypesManager.KnownTypeDestinationRelations[p.Property] != null
                    );

            graph.Paths.RemoveWhere(p=>notExplicitDsts.Contains(p));
            
            //IEnumerable<Path> explicitDsts = graph.Paths.Where(p =>
            //        typeof(ExplicitConnection).IsAssignableFrom(TypeSystem.GetElementType(p.Property.PropertyType)) &&
            //        OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(p.Property) &&
            //        OrmCoreTypes.KnownTypeDestinationRelations[p.Property] != null
            //        );

            //graph.Paths.RemoveWhere(p => explicitDsts.Contains(p));
        }
        
        private static void FixExplicitConnectionSource(StoringGraph graph, Path path)
        {
            foreach (ExplicitConnection obj in path.Targets.Select(p => (ExplicitConnection)p))
            {
                if (obj.Destination == null)
                    throw new Exception("Missing destination vertex of explicit connection.");
                if (obj.Source == null)
                {
                    obj.Source = path.Origin;
                    graph.Paths.Add(new Path(graph, obj.GetType().GetProperty(nameof(obj.Source), obj.GetType()), obj, new[] { path.Origin }));

                    FixExplicitConnectionSourceWiring(graph, path);
                }
                else if (obj.Source != path.Origin)
                    throw new Exception("Reference conflict. Source is different from the navigated one.");
            }
        }
        private static void FixExplicitConnectionDestination(StoringGraph graph, Path path)
        {
            foreach (ExplicitConnection obj in path.Targets.Select(p => (ExplicitConnection)p))
            {
                if (obj.Source == null)
                    throw new Exception("Missing Source vertex of explicit connection.");
                if (obj.Destination == null)
                {
                    obj.Destination = path.Origin;
                    graph.Paths.Add(new Path(graph, obj.GetType().GetProperty(nameof(obj.Destination), obj.GetType()), obj, new[] { path.Origin }));

                    FixImplicitConnectionDestinationWiring(graph, path);
                }
                else if (obj.Destination != path.Origin)
                    throw new Exception("Reference conflict. Destination is different from the navigated one.");
            }
        }
        private static void FixExplicitConnectionSame(StoringGraph graph, Path path)
        {
            foreach (ExplicitConnection obj in path.Targets.Select(p => (ExplicitConnection)p))
            {
                if (obj.Source == null && obj.Destination == null)
                    throw new Exception("Missing every vertex of explicit connection.");

                if (obj.Source == null)
                {
                    FixExplicitConnectionSource(graph, path);
                }
                else if (obj.Destination == null)
                {
                    FixExplicitConnectionDestination(graph, path);
                }
                else if (obj.Destination != path.Origin && obj.Source != path.Origin)
                    throw new Exception("Reference conflict. Both Source and Destination are different from the navigated one.");
            }
        }
        private static void CheckExplicitConnection(StoringGraph graph, Path path)
        {
            if (TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property))
            {
                PropertyInfo pinfo = TypesManager.KnownTypeSourceRelations[path.Property];
                if(pinfo!=null)
                {
                    foreach (ExplicitConnection obj in path.Targets.Select(p => (ExplicitConnection)p))
                    {
                        if (!graph.Paths.Any(p => p.Property == pinfo && p.Targets.Contains(obj) && obj.Destination == p.Origin))
                            throw new Exception("Explicit connection refernces anomaly detected.");
                    }
                }
            }
            else if (TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property))
            {
                PropertyInfo pinfo = TypesManager.KnownTypeDestinationRelations[path.Property];
                if (pinfo != null)
                {
                    foreach (ExplicitConnection obj in path.Targets.Select(p => (ExplicitConnection)p))
                    {
                        if (!graph.Paths.Any(p => p.Property == pinfo && p.Targets.Contains(obj) && obj.Source == p.Origin))
                            throw new Exception("Explicit connection refernces anomaly detected.");
                    }
                }
            }
        }

        private static void FixNullImplicitConnection(StoringGraph graph, Path path, object dstOrigin, PropertyInfo pinfo)
        {
            if (ObjectExtensions.HasEnumerable(pinfo.PropertyType))
            {
                if (!typeof(IList).IsAssignableFrom(pinfo.PropertyType))
                    throw new Exception("The propery is immutable. Unable to fix connections.");
                else
                {
                    if (ObjectExtensions.Configuration.Get(pinfo,dstOrigin) == null)
                        ObjectExtensions.Configuration.Set(pinfo,dstOrigin, Activator.CreateInstance(pinfo.PropertyType));

                    IList lst = (IList)ObjectExtensions.Configuration.Get(pinfo,dstOrigin);
                    lst.Add(path.Origin);
                    List<object> objs = new List<object>();
                    foreach (object item in lst)
                        objs.Add(item);
                    graph.Paths.Add(new Path(graph, pinfo, dstOrigin, objs));
                }
            }
            else
            {
                ObjectExtensions.Configuration.Set(pinfo,dstOrigin, path.Origin);
                graph.Paths.Add(new Path(graph, pinfo, dstOrigin, new[] { path.Origin }));
            }
        }
        private static void FixMissingImplicitConnection(StoringGraph graph, Path path, Path errorPath, object dstOrigin, PropertyInfo pinfo)
        {
            if (ObjectExtensions.HasEnumerable(pinfo.PropertyType))
            {
                if (!typeof(IList).IsAssignableFrom(pinfo.PropertyType))
                    throw new Exception("The propery is immutable. Unable to fix connections.");
                else
                {
                    IList lst = (IList)ObjectExtensions.Configuration.Get(pinfo,dstOrigin);
                    lst.Add(path.Origin);
                    List<object> objs = new List<object>();
                    foreach (object item in lst)
                        objs.Add(item);
                    graph.Paths.Add(new Path(graph, pinfo, dstOrigin, objs));
                    graph.Paths.Remove(errorPath);
                }
            }
            else
            {
                ObjectExtensions.Configuration.Set(pinfo,dstOrigin, path.Origin);
                graph.Paths.Add(new Path(graph, pinfo, dstOrigin, new[] { path.Origin }));
                graph.Paths.Remove(errorPath);
            }
        }
        private static void FixImplicitConnectionSource(StoringGraph graph, Path path)
        {
            PropertyInfo pinfo = TypesManager.KnownTypeSourceRelations[path.Property];
            if (pinfo != null)
            {
                foreach (object obj in path.Targets)
                {
                    Path tmp = graph.Paths.FirstOrDefault(p=>p.Property == pinfo && p.Origin == obj);
                    if (tmp == null)
                    {
                        FixNullImplicitConnection(graph, path, obj, pinfo);
                    }
                    else if(!tmp.Targets.Contains(path.Origin))
                    {
                        FixMissingImplicitConnection(graph, path, tmp, obj, pinfo);
                    }
                }
            }
        }
        private static void FixExplicitConnectionSourceWiring(StoringGraph graph, Path path)
        {
            PropertyInfo pinfo = TypesManager.KnownTypeSourceRelations[path.Property];
            if (pinfo != null)
            {
                foreach (object obj in path.Targets)
                {
                    ExplicitConnection ec = obj as ExplicitConnection;
                    Path tmp = graph.Paths.FirstOrDefault(p => p.Property == pinfo && p.Origin == ec.Destination);
                    if (tmp == null)
                    {
                        FixNullImplicitConnection(graph,
                            graph.Paths.First(p =>
                            p.Property == obj.GetType().GetProperty(nameof(ec.Destination), obj.GetType()) &&
                            p.Origin == obj), ec.Destination, pinfo);
                    }
                    else if (!tmp.Targets.Contains(obj))
                    {
                        FixMissingImplicitConnection(graph, 
                            graph.Paths.First(p=>
                            p.Property==obj.GetType().GetProperty(nameof(ec.Destination), obj.GetType()) && 
                            p.Origin == obj), tmp, ec.Destination, pinfo);
                    }
                }
            }
        }
        private static void FixImplicitConnectionDestinationWiring(StoringGraph graph, Path path)
        {
            PropertyInfo pinfo = TypesManager.KnownTypeDestinationRelations[path.Property];
            if (pinfo != null)
            {
                foreach (object obj in path.Targets)
                {
                    ExplicitConnection ec = obj as ExplicitConnection;
                    Path tmp = graph.Paths.FirstOrDefault(p => p.Property == pinfo && p.Origin == ec.Source);
                    if (tmp == null)
                    {
                        FixNullImplicitConnection(graph,
                            graph.Paths.First(p =>
                            p.Property == obj.GetType().GetProperty(nameof(ec.Source), obj.GetType()) &&
                            p.Origin == obj), ec.Source, pinfo);
                    }
                    else if (!tmp.Targets.Contains(path.Origin))
                    {
                        FixMissingImplicitConnection(graph,
                            graph.Paths.First(p =>
                            p.Property == obj.GetType().GetProperty(nameof(ec.Source), obj.GetType()) &&
                            p.Origin == obj), tmp, ec.Source, pinfo);
                    }
                }
            }
        }
        private static void FixImplicitConnectionDestination(StoringGraph graph, Path path)
        {
            PropertyInfo pinfo = TypesManager.KnownTypeDestinationRelations[path.Property];
            if (pinfo != null)
            {
                foreach (object obj in path.Targets)
                {
                    Path tmp = graph.Paths.FirstOrDefault(p => p.Property == pinfo && p.Origin == obj);
                    if (tmp == null)
                    {
                        FixNullImplicitConnection(graph, path, obj, pinfo);
                    }
                    else if (!tmp.Targets.Contains(path.Origin))
                    {
                        FixMissingImplicitConnection(graph, path, tmp, obj, pinfo);
                    }
                }
            }
        }
        private static void CheckImplicitConnection(StoringGraph graph, Path path)
        {
            if (TypesManager.KnownTypeSourceRelations.ContainsKey(path.Property))
            {
                PropertyInfo pinfo = TypesManager.KnownTypeSourceRelations[path.Property];
                if (pinfo != null)
                {
                    foreach (object obj in path.Targets)
                    {
                        if (!graph.Paths.Any(p => p.Property == pinfo && p.Targets.Contains(path.Origin)))
                            throw new Exception("Implicit connection refernces anomaly detected.");
                    }
                }
            }
            else if (TypesManager.KnownTypeDestinationRelations.ContainsKey(path.Property))
            {
                PropertyInfo pinfo = TypesManager.KnownTypeDestinationRelations[path.Property];
                if (pinfo != null)
                {
                    foreach (object obj in path.Targets)
                    {
                        if (!graph.Paths.Any(p => p.Property == pinfo && p.Targets.Contains(path.Origin)))
                            throw new Exception("Implicit connection refernces anomaly detected.");
                    }
                }
            }
        }
    }
}
