using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public interface IGraphManagedStatementRunner
    {//TODO: usalo per creare uno pseudo dbcontext
        N4pperManager Manager { get; }
    }
}
