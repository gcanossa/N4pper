using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm
{
    public interface IOrmStatementRunner : IGraphManagedStatementRunner
    {
        GraphContext Context { get; }
    }
}
