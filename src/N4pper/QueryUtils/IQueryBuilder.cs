using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public interface IQueryBuilder
    {
        Symbol Symbol(string name = null);

        INode Node<T>(Symbol symbol, object param) where T : class;
        INode Node<T>(Symbol symbol, Dictionary<string, object> param = null) where T : class;
        INode Node(Symbol symbol);
        IRel Rel<T>(Symbol symbol, object param) where T : class;
        IRel Rel<T>(Symbol symbol, Dictionary<string, object> param = null) where T : class;
        IRel Rel(Symbol symbol);

        IEnumerable<Symbol> Symbols { get; }
    }
}
