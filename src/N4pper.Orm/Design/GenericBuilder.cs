using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using OMnG;

namespace N4pper.Orm.Design
{
    internal class GenericBuilder<T> : IConstraintBuilder<T> where T : class
    {
        private void ManageConnection<D>(IEnumerable<string> from, IEnumerable<string> back)
        {
            PropertyInfo f = typeof(T).GetProperty(from.First());
            PropertyInfo b = typeof(D).GetProperty(back.First());

            if (!OrmCoreTypes.KnownTypeRelations.ContainsKey(f))
            {
                OrmCoreTypes.KnownTypeRelations.Add(f, b);
            }
            if (!OrmCoreTypes.KnownTypeRelations.ContainsKey(b))
            {
                OrmCoreTypes.KnownTypeRelations.Add(b, f);
            }
        }
        public void Connected<D>(Expression<Func<T, D>> from, Expression<Func<D, object>> back) where D : class
        {
            from = from ?? throw new ArgumentNullException(nameof(from));
            back = back ?? throw new ArgumentNullException(nameof(back));

            IEnumerable<string> fromP = from.ToPropertyNameCollection();
            if (fromP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(from));

            IEnumerable<string> backP = back.ToPropertyNameCollection();
            if (backP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(back));

            ManageConnection<D>(fromP, backP);
        }
        
        public void ConnectedMany<D>(Expression<Func<T, IEnumerable<D>>> from, Expression<Func<D, object>> back) where D : class
        {
            from = from ?? throw new ArgumentNullException(nameof(from));
            back = back ?? throw new ArgumentNullException(nameof(back));

            IEnumerable<string> fromP = from.ToPropertyNameCollection();
            if (fromP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(from));

            IEnumerable<string> backP = back.ToPropertyNameCollection();
            if (backP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(back));

            ManageConnection<D>(fromP, backP);
        }
        
        public IPropertyConstraintBuilder<T> Ignore(Expression<Func<T, object>> expr)
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            foreach (string item in expr.ToPropertyNameCollection())
            {
                if (OrmCoreTypes.KnownTypes[typeof(T)].Contains(item))
                    throw new InvalidOperationException("It's not possible to ignore a key property.");
                if (!OrmCoreTypes.KnownTypesIngnoredProperties[typeof(T)].Contains(item))
                    OrmCoreTypes.KnownTypesIngnoredProperties[typeof(T)].Add(item);
            }

            return this;
        }
    }
}
