using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public interface IGraphManagedStatementRunner
    {
        N4pperManager Manager { get; }
        bool IsApocAvailable { get; }
    }
}
