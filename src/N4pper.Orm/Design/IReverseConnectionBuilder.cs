using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    public interface IReverseConnectionBuilder<T, D> where T : class where D : class
    {
        void Connected(Expression<Func<T, D>> back);
        void ConnectedMany(Expression<Func<T, IEnumerable<D>>> back);
    }
}
