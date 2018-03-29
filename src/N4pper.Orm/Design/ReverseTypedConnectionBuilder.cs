using N4pper.Orm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    internal class ReverseTypedConnectionBuilder<T, C, D> : IReverseTypedConnectionBuilder<T, C, D> where C : ExplicitConnection<D, T> where T : class where D : class
    {
        protected Action<IEnumerable<string>> Handler { get; set; }
        public ReverseTypedConnectionBuilder(Action<IEnumerable<string>> handler)
        {
            Handler = handler;
        }
        public void Connected(Expression<Func<T, C>> destination)
        {
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(typeof(C)) && typeof(C).BaseType.GetGenericTypeDefinition() != typeof(Entities.ExplicitConnection<,>))
                throw new ArgumentException($"An explicit connection must inherit directly from {typeof(Entities.ExplicitConnection<,>).Name}");

            destination = destination ?? throw new ArgumentNullException(nameof(destination));

            IEnumerable<string> backP = destination.ToPropertyNameCollection();
            if (backP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(destination));

            Handler(backP);
        }

        public void ConnectedMany(Expression<Func<T, IEnumerable<C>>> destination)
        {
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(typeof(C)) && typeof(C).BaseType.GetGenericTypeDefinition() != typeof(Entities.ExplicitConnection<,>))
                throw new ArgumentException($"An explicit connection must inherit directly from {typeof(Entities.ExplicitConnection<,>).Name}");

            destination = destination ?? throw new ArgumentNullException(nameof(destination));

            IEnumerable<string> backP = destination.ToPropertyNameCollection();
            if (backP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(destination));

            Handler(backP);
        }
    }
}
