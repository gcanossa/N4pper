using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Ogm.Design
{
    public interface IPropertyConstraintBuilder<T> where T : class, Entities.IOgmEntity
    {
        IPropertyConstraintBuilder<T> Ignore(Expression<Func<T, object>> expr);
    }
}
