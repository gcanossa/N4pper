using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Design
{
    public interface IConstraintBuilder<T> : 
        IConnectionBuilder<T>, 
        ITypedConnectionBuilder<T>,
        IPropertyConstraintBuilder<T> where T : class, IOgmEntity
    {
    }
}
