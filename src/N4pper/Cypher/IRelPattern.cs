using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Cypher
{
    public interface IRelPattern : IPattern
    {
        INodePattern _X { get; }
        INodePattern _ { get; }

        IRelPattern PathLength(int? min = null, int? max = null);
        IRelPattern Symbol(Symbol symbol);
        IRelPattern SetType(string type);

        IRelPattern Rel();
        IRelPattern Rel(object rel, object values = null);
        IRelPattern Rel<T>() where T : class;
        IRelPattern Rel<T>(T rel, object param = null) where T : class;
        IRelPattern Rel<T>(Expression<Func<T, object>> expr, object values = null, object param = null) where T : class;
    }
}
