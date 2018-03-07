using AsIKnow.Graph;
using N4pper.Diagnostic;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public interface IGraphManagedStatementRunner
    {
        GraphManager Manager { get; }
        N4pperOptions Options { get; }

        IQueryTracer Tracer { get; }
    }
}
