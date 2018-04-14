using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Ogm.Design
{
    public interface IReverseTypedConnectionBuilder<T, C, D> 
        where C : class, IOgmConnection<D, T> 
        where T : class, IOgmEntity
        where D : class, IOgmEntity
    {
        void Connected(Expression<Func<T, C>> destination);
        void ConnectedMany(Expression<Func<T, IEnumerable<C>>> destination);
    }
}
