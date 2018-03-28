using N4pper.Orm.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    public interface IReverseTypedConnectionBuilder<T, C, D> where C : ExplicitConnection<D, T> where T : class where D : class
    {
        void Connected(Expression<Func<T, C>> destination);
        void ConnectedMany(Expression<Func<T, IEnumerable<C>>> destination);
    }
}
