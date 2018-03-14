using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher
{
    public interface IStatementBuilder
    {
        IStatementBuilder Previous { get; }
        string Build();
    }
}
