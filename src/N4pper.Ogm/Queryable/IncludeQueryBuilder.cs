//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using N4pper.Ogm.Design;
//using N4pper.QueryUtils;
//using OMnG;

//namespace N4pper.Ogm.Queryable
//{
//    internal class IncludeQueryBuilder<T> : IInclude<T> where T : class, Entities.IOgmEntity
//    {
//        protected ITree<IncludePathComponent> Path { get; set; }

//        public IncludeQueryBuilder(ITree<IncludePathComponent> path)
//        {
//            Path = path ?? throw new ArgumentNullException(nameof(path));
//        }

//        private ITree<IncludePathComponent> ManageInclude<D>(IEnumerable<string> props, bool isEnumerable, string label = null)
//        {
//            if(props.Count()!=1)
//                throw new ArgumentException("Only a single navigation property must be specified", nameof(props));
//            PropertyInfo pinfo = typeof(T).GetProperty(props.First());

//            PropertyInfo sPinfo =
//                TypesManager.KnownTypeSourceRelations.ContainsKey(pinfo) ?
//                pinfo :
//                TypesManager.KnownTypeDestinationRelations.ContainsKey(pinfo) ?
//                TypesManager.KnownTypeDestinationRelations[pinfo] ?? null :
//                pinfo;
//            PropertyInfo dPinfo =
//                TypesManager.KnownTypeSourceRelations.ContainsKey(sPinfo) ?
//                TypesManager.KnownTypeSourceRelations[sPinfo] ?? null :
//                null;

//            bool isReverse = TypesManager.KnownTypeDestinationRelations.ContainsKey(pinfo) && !TypesManager.KnownTypeSourceRelations.ContainsKey(pinfo);

//            Symbol to = new Symbol();
//            Symbol toRel = new Symbol();
//            ITree<IncludePathComponent> newTree = new IncludePathTree()
//            {
//                Item = new IncludePathComponent()
//                {
//                    IsReverse = isReverse,
//                    SourceProperty = sPinfo,
//                    DestinationProperty = dPinfo,
//                    IsEnumerable = isEnumerable,
//                    Symbol = to,
//                    RelSymbol = toRel,
//                    Label = label
//                }
//            };

//            newTree = Path.Add(newTree);

//            return newTree;
//        }

//        public IInclude<D> Include<D>(Expression<Func<T, D>> expr) where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), false));
//        }

//        public IInclude<D> Include<D>(Expression<Func<T, IEnumerable<D>>> expr) where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true));
//        }

//        public IInclude<D> Include<D>(Expression<Func<T, IList<D>>> expr) where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true));
//        }

//        public IInclude<D> Include<D>(Expression<Func<T, List<D>>> expr) where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true));
//        }

//        public IInclude<D> Include<C, D>(Expression<Func<T, C>> expr)
//            where C : Entities.ExplicitConnection<T, D>
//            where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), false, OMnG.TypeExtensions.GetLabel(typeof(C))));
//        }

//        public IInclude<D> Include<C, D>(Expression<Func<T, IEnumerable<C>>> expr)
//            where C : Entities.ExplicitConnection<T, D>
//            where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true, OMnG.TypeExtensions.GetLabel(typeof(C))));
//        }

//        public IInclude<D> Include<C, D>(Expression<Func<T, IList<C>>> expr)
//            where C : Entities.ExplicitConnection<T, D>
//            where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true, OMnG.TypeExtensions.GetLabel(typeof(C))));
//        }

//        public IInclude<D> Include<C, D>(Expression<Func<T, List<C>>> expr)
//            where C : Entities.ExplicitConnection<T, D>
//            where D : class, Entities.IOgmEntity
//        {
//            expr = expr ?? throw new ArgumentNullException(nameof(expr));

//            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true, OMnG.TypeExtensions.GetLabel(typeof(C))));
//        }
//    }
//}
