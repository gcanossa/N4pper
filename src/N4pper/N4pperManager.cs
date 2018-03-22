using N4pper.Diagnostic;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace N4pper
{
    public class N4pperManager
    {
        public N4pperOptions Options { get; protected set; }

        protected IQueryProfiler Profiler { get; set; }

        public N4pperManager(N4pperOptions options, IQueryProfiler profiler)
        {
            Options = options;
            Profiler = profiler;
        }

        public void TraceStatement(string statement)
        {
            Profiler?.Trace(statement);
            Profiler?.Increment();
        }

        public IStatementResult ProfileQuery(Func<IStatementResult> query)
        {
            Action<Exception> time = Profiler?.Mark();
            try
            {
                IStatementResult r = query();
                time?.Invoke(null);
                return r;
            }
            catch(Exception e)
            {
                time?.Invoke(e);
                throw e;
            }
        }
        public Task<IStatementResultCursor> ProfileQueryAsync(Func<Task<IStatementResultCursor>> query)
        {
            Action<Exception> time = Profiler?.Mark();
            try
            {
                Task<IStatementResultCursor> r = query();
                r.ContinueWith(p =>
                {
                    if(p.Exception == null)
                        time?.Invoke(null);
                    else
                        time?.Invoke(p.Exception);
                });
                return r;
            }
            catch (Exception e)
            {
                time?.Invoke(e);
                throw e;
            }
        }
    }
}
