using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    public interface IPropertyConstraintBuilder<T> where T : class
    {
        IPropertyConstraintBuilder<T> Ignore(Expression<Func<T, object>> expr);
    }
}
