using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher
{
    public interface IPattern : IStatementBuilder
    {
        IEnumerable<Symbol> Symbols { get; }
    }
}
