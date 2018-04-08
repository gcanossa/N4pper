using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Ogm.Design
{
    public interface IConnectionBuilder<T> where T : class, IOgmEntity
    {
        IReverseConnectionBuilder<D, T> Connected<D>(Expression<Func<T,D>> from = null) where D : class, IOgmEntity;
        IReverseConnectionBuilder<D, T> ConnectedMany<D>(Expression<Func<T, IEnumerable<D>>> from = null) where D : class, IOgmEntity;
    }
}
