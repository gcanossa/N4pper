using AsIKnow.Graph;
using N4pper.Diagnostic;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public class N4pperManager
    {
        public  GraphManager Manager { get; protected set; }
        public N4pperOptions Options { get; protected set; }

        protected IQueryTracer Tracer { get; set; }

        public N4pperManager(GraphManager manager, N4pperOptions options, IQueryTracer tracer)
        {
            Manager = manager;
            Options = options;
            Tracer = tracer;
        }

        public void TraceStatement(string statement)
        {
            Tracer?.Trace(statement);
        }
    }
}
