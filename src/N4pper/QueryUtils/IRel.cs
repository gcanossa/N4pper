using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public interface IRel : IStatementBuilder
    {
        IRel Lengths(int? min = null, int? max = null);

        string Label { get; }

        IRel SetSymbol(Symbol symbol);
        IRel SetType(Type type);

        NodePath _(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null);
        NodePath _V(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null);
        NodePath _(INode node);
        NodePath _V(INode node);
    }
}
