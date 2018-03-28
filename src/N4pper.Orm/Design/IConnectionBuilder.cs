using N4pper.Orm.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    public interface IConnectionBuilder<T> where T : class
    {
        IReverseConnectionBuilder<D, T> Connected<D>(Expression<Func<T,D>> from = null) where D : class;
        IReverseConnectionBuilder<D, T> ConnectedMany<D>(Expression<Func<T, IEnumerable<D>>> from = null) where D : class;
    }
}
