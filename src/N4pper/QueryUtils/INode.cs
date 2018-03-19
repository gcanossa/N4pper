using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public interface INode : IStatementBuilder
    {
        string Labels { get; }

        INode SetSymbol(Symbol symbol);
        INode SetType(Type type);

        RelPath _(Symbol symbol = null, Type type = null, Dictionary<string, object> props = null);
        RelPath V_(Symbol symbol = null, Type type = null, Dictionary<string, object> props = null);
        RelPath _(IRel rel);
        RelPath V_(IRel rel);
    }
}
