using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Diagnostic
{
    public class DebugQueryProfiler : IQueryProfiler
    {
        private ILogger<DebugQueryProfiler> _logger;

        public DebugQueryProfiler(ILogger<DebugQueryProfiler> logger)
        {
            _logger = logger;
        }

        public Action<Exception> Mark(string query)
        {
            string uuid = Guid.NewGuid().ToString("N");
            _logger.LogDebug($"Query begin: {uuid}, {query}");
            Action<Exception> r = e => 
            {
                if(e==null)
                    _logger.LogDebug($"Query success: {uuid}");
                else
                    _logger.LogDebug(e, $"Query failure: {uuid}");
            };

            return r;
        }
    }
}
