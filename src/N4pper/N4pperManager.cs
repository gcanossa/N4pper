using N4pper.Diagnostic;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public class N4pperManager
    {
        public N4pperOptions Options { get; protected set; }

        protected IQueryTracer Tracer { get; set; }

        public N4pperManager(N4pperOptions options, IQueryTracer tracer)
        {
            Options = options;
            Tracer = tracer;
        }

        public void TraceStatement(string statement)
        {
            Tracer?.Trace(statement);
        }
    }
}
