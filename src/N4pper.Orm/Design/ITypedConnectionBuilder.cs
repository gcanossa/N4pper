using N4pper.Orm.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    public interface ITypedConnectionBuilder<T> where T : class
    {
        IReverseTypedConnectionBuilder<D, C, T> ConnectedWith<C, D>(Expression<Func<T, C>> source = null) where C : ExplicitConnection<T, D> where D : class;
        IReverseTypedConnectionBuilder<D, C, T> ConnectedManyWith<C, D>(Expression<Func<T, IEnumerable<C>>> source = null) where C : ExplicitConnection<T, D> where D : class;
    }
}
