using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    public interface IConnectionBuilder<T> where T : class
    {
        void Connected<D>(Expression<Func<T,D>> from, Expression<Func<D,object>> back) where D : class;
        void ConnectedMany<D>(Expression<Func<T, IEnumerable<D>>> from, Expression<Func<D, object>> back) where D : class;
    }
}
