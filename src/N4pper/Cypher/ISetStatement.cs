using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Cypher
{
    public interface ISetStatement
    {
        ISetStatement Body(object obj, object values = null);
        ISetStatement Body<T>(T obj, object param = null) where T : class;
        ISetStatement Body<T>(Expression<Func<T, object>> expr, object values = null, object param = null) where T : class;
    }
}
