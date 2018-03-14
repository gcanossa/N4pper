using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher
{
    public interface IPathClauseStatement
    {
        IPathClauseStatement Set(Symbol symbol, Action<ISetStatement> statement);
    }
}
