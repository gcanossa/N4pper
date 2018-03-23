using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public interface INode : IStatementBuilder
    {
        string Labels { get; }

        Symbol Symbol { get; }
        Type Type { get; }
        IDictionary<string, object> Props { get; }

        INode SetSymbol(Symbol symbol);
        INode SetType(Type type);

        RelPath _(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null);
        RelPath V_(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null);
        RelPath _(IRel rel);
        RelPath V_(IRel rel);
    }
}
