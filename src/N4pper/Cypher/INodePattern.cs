using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Cypher
{
    public interface INodePattern : IPattern
    {
        IRelPattern X_ { get; }
        IRelPattern _ { get; }

        INodePattern Symbol(Symbol symbol);
        INodePattern SetLabels(params string[] labels);

        INodePattern Node();
        INodePattern Node(object node, object values = null);
        INodePattern Node<T>() where T : class;
        INodePattern Node<T>(T node, object param = null) where T : class;
        INodePattern Node<T>(Expression<Func<T, object>> expr, object values = null, object param = null) where T : class;
    }
}
