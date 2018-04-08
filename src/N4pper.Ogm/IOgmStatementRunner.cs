using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm
{
    public interface IOgmStatementRunner : IGraphManagedStatementRunner
    {
        GraphContext Context { get; }
    }
}
