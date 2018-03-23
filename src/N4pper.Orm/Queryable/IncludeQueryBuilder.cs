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
        public StringBuilder Builder { get; set; }
        public List<string> ReturnStatements { get; set; }

        public IncludeQueryBuilder(StringBuilder builder, List<string> returnStatements)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            ReturnStatements = returnStatements ?? throw new ArgumentNullException(nameof(returnStatements));
        }

        private Symbol ManageInclude<D>(IEnumerable<string> props)
        {
            if(props.Count()!=1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(props));
            PropertyInfo pinfo = typeof(T).GetProperty(props.First());

            Symbol to = new Symbol();
            Rel rel = new Rel(
                null, 
                typeof(Entities.Connection), 
                new { PropertyName = pinfo.Name }.ToPropDictionary());
            Node node = new Node(to, typeof(D));

            Builder.Append($"-{rel}->{node}");

            return to;
        }

        public IInclude<D> Include<D>(Expression<Func<T, D>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            Symbol s = ManageInclude<D>(expr.ToPropertyNameCollection());

            ReturnStatements.Add($"{s}");

            return new IncludeQueryBuilder<D>(Builder, ReturnStatements);
        }

        public IInclude<D> Include<D>(Expression<Func<T, IEnumerable<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            Symbol s = ManageInclude<D>(expr.ToPropertyNameCollection());

            ReturnStatements.Add($"reverse(collect({s}))");

            return new IncludeQueryBuilder<D>(Builder, ReturnStatements);
        }

        public IInclude<D> Include<D>(Expression<Func<T, IList<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            Symbol s = ManageInclude<D>(expr.ToPropertyNameCollection());

            ReturnStatements.Add($"reverse(collect({s}))");

            return new IncludeQueryBuilder<D>(Builder, ReturnStatements);
        }

        public IInclude<D> Include<D>(Expression<Func<T, List<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            Symbol s = ManageInclude<D>(expr.ToPropertyNameCollection());

            ReturnStatements.Add($"reverse(collect({s}))");

            return new IncludeQueryBuilder<D>(Builder, ReturnStatements);
        }
    }
}
