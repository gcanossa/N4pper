using N4pper.Decorators;
using N4pper.Orm.Design;
using N4pper.Orm.Queryable;
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
            OrmCoreTypes.Entity<Entities.Connection>(p => new { p.PropertyName, p.Version });
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

        #region private methods

        private (List<Tuple<PropertyInfo, int, IEnumerable<int>>>, List<object>) PrepareStoringObjectGraph(IEnumerable<object> objects)
        {
            List<Tuple<PropertyInfo, int, IEnumerable<int>>> result = new List<Tuple<PropertyInfo, int, IEnumerable<int>>>();

            List<object> index = new List<object>();

            if (objects != null)
            {
                foreach (object item in objects)
                {
                    TraverseStoringObject(item, result, index);
                }
            }

            return (result, index);
        }
        private void TraverseStoringObject(object obj, List<Tuple<PropertyInfo, int, IEnumerable<int>>> accumulator, List<object> index)
        {
            if (index.Contains(obj))
                return;
            index.Add(obj);

            foreach (PropertyInfo pInfo in obj.GetType().GetProperties()
                .Where(p=>!ObjectExtensions.IsPrimitive(p.PropertyType) && p.GetValue(obj)!=null))
            {
                Type ienumerable = pInfo.PropertyType.GetInterface("IEnumerable`1");
                if (ienumerable != null && !ObjectExtensions.IsPrimitive(ienumerable.GetGenericArguments()[0]))
                {
                    object tmp = pInfo.GetValue(obj);
                    List<int> idx = new List<int>();
                    foreach (object value in ((IEnumerable)tmp))
                    {
                        TraverseStoringObject(value, accumulator, index);
                    }
                    foreach (object value in ((IEnumerable)tmp))
                    {
                        idx.Add(index.IndexOf(value));
                    }
                    accumulator.Add(new Tuple<PropertyInfo, int, IEnumerable<int>>(pInfo, index.IndexOf(obj), idx));
                }
                else
                {
                    object value = pInfo.GetValue(obj);
                    TraverseStoringObject(value, accumulator, index);
                    accumulator.Add(new Tuple<PropertyInfo, int, IEnumerable<int>>(pInfo, index.IndexOf(obj), new int[] { index.IndexOf(value) }));
                }
            }
        }
        
        private void ValidateStoringObjectGraph(List<Tuple<PropertyInfo, int, IEnumerable<int>>> graph, List<object> index)
        {
            if (index.Any(p => ManagedObjectsToBeRemoved.Contains(p)))
                throw new InvalidOperationException("Some object has some references marked to be deleted.");

            foreach (Tuple<PropertyInfo, int, IEnumerable<int>> item in graph)
            {
                PropertyInfo connection = OrmCoreTypes.KnownTypeRelations.ContainsKey(item.Item1) ? OrmCoreTypes.KnownTypeRelations[item.Item1]:null;
                if(connection!=null)
                {
                    foreach (int idx in item.Item3)
                    {
                        bool fail = true;

                        foreach (Tuple<PropertyInfo, int, IEnumerable<int>> p in graph.Where(p=>p.Item1==connection && p.Item2==idx))
                        {
                            fail = !p.Item3.Contains(item.Item2);
                            if (!fail)
                                break;
                        }

                        if (fail)
                            throw new InvalidOperationException($"Property reference constraint violation detected. {item.Item1.ReflectedType.FullName}.{item.Item1.Name}->{connection.ReflectedType.FullName}.{connection.Name}");
                    }
                }
            }
        }

        #endregion

        private static readonly MethodInfo _addRel = typeof(OrmCore).GetMethods().First(p => p.Name == nameof(OrmCore.AddOrUpdateRel) && p.GetGenericArguments().Length == 3);

        private Dictionary<string, MethodInfo> AddRel { get; } = new Dictionary<string, MethodInfo>();

        private void AddOrUpdate(IStatementRunner runner)
        {
            (List<Tuple<PropertyInfo, int, IEnumerable<int>>> graph, List<object> index) = PrepareStoringObjectGraph(ManagedObjects);

            ValidateStoringObjectGraph(graph, index);

            for (int i = 0; i < index.Count; i++)
            {
                MethodInfo m = OrmCoreTypes.AddNode[index[i].GetType()];
                MethodInfo c = OrmCoreTypes.CopyProps[index[i].GetType()];
                index[i] = c.Invoke(null, new object[] { index[i], m.Invoke(null, new object[] { runner, index[i] }).ToPropDictionary().SelectProperties(OrmCoreTypes.KnownTypes[index[i].GetType()]), null });
            }

            foreach (Tuple<PropertyInfo, int, IEnumerable<int>> item in graph)
            {
                long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int relOrder = item.Item3.Count();
                foreach (int idx in item.Item3.Reverse())
                {
                    string key = $"{index[item.Item2].GetType().FullName}:{index[idx].GetType().FullName}";
                    if (!AddRel.Any(p => p.Key == key))
                        AddRel.Add(key, _addRel.MakeGenericMethod(typeof(Entities.Connection), index[item.Item2].GetType(), index[idx].GetType()));
                    MethodInfo m = AddRel[key];

                    m.Invoke(null, new object[] { runner, new Entities.Connection() { PropertyName = item.Item1.Name, Order = relOrder--, Version = version }, index[item.Item2], index[idx] });
                }
                runner.Execute(p =>
                    $"MATCH {new Node(p.Symbol(), index[item.Item2].GetType(), index[item.Item2]?.SelectProperties(OrmCoreTypes.KnownTypes[item.Item1.ReflectedType]))}" +
                    $"-{p.Rel<Entities.Connection>(p.Symbol("r"), new { PropertyName = item.Item1.Name })}->" +
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
