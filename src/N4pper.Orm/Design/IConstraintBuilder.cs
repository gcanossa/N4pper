using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm.Design
{
    public interface IConstraintBuilder<T> : 
        IConnectionBuilder<T>, 
        ITypedConnectionBuilder<T>,
        IPropertyConstraintBuilder<T> where T : class
    {
    }
}
