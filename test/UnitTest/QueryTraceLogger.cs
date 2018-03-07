using Microsoft.Extensions.Logging;
using N4pper.Diagnostic;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest
{
    public class QueryTraceLogger : IQueryTracer
    {
        protected ILogger Logger { get; set; }
        public QueryTraceLogger(ILogger<QueryTraceLogger> logger)
        {
            Logger = logger;
        }
        public void Trace(string query)
        {
            Logger.LogDebug(query);
        }
    }
}
