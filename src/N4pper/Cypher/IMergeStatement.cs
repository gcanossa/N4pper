using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher
{
    public interface IMergeOnCreateStatement
    {
        IMergeOnMatchStatement OnMatch(Symbol symbol, Action<ISetStatement> statement);
    }
    public interface IMergeOnMatchStatement
    {
        IMergeOnCreateStatement OnCreate(Symbol symbol, Action<ISetStatement> statement);
    }
    public interface IMergeStatement : IMergeOnCreateStatement, IMergeOnMatchStatement
    {
    }
}
