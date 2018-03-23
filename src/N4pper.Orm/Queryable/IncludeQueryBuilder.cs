using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using N4pper.QueryUtils;
using OMnG;

namespace N4pper.Orm.Queryable
{
    internal class IncludeQueryBuilder<T> : IInclude<T> where T : class
    {
        protected IncludePathTree Path { get; set; }

        public IncludeQueryBuilder(IncludePathTree path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        private IncludePathTree ManageInclude<D>(IEnumerable<string> props, bool isEnumerable)
        {
            if(props.Count()!=1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(props));
            PropertyInfo pinfo = typeof(T).GetProperty(props.First());

            Symbol to = new Symbol();
            IncludePathTree newTree = new IncludePathTree()
            {
                Path = new IncludePathComponent() { Property = pinfo, IsEnumerable = isEnumerable, Symbol = to }
            };

            newTree = Path.Add(newTree);

            return newTree;
        }

        public IInclude<D> Include<D>(Expression<Func<T, D>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), false));
        }

        public IInclude<D> Include<D>(Expression<Func<T, IEnumerable<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true));
        }

        public IInclude<D> Include<D>(Expression<Func<T, IList<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true));
        }

        public IInclude<D> Include<D>(Expression<Func<T, List<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return new IncludeQueryBuilder<D>(ManageInclude<D>(expr.ToPropertyNameCollection(), true));
        }
    }
}
