using N4pper.Orm;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public interface IGraphManagedStatementRunner
    {
        GraphContext Context { get; }
        N4pperManager Manager { get; }
    }
}
