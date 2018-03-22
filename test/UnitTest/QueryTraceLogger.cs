using Microsoft.Extensions.Logging;
using N4pper.Diagnostic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnitTest
{
    public class QueryTraceLogger : IQueryProfiler
    {
        protected ILogger Logger { get; set; }
        public static string LastStatement { get; private set; }

        public static int QueryCount { get; private set; } = 0;
        public static int SuccessCount { get; private set; } = 0;
        public static double SuccessTimeAvg { get; private set; } = 0;
        public static double SuccessTimeLast { get; private set; } = 0;
        public static int ErrorCount { get; private set; } = 0;
        public static double ErrorTimeAvg { get; private set; } = 0;
        public static double ErrorTimeLast { get; private set; } = 0;
        public static Exception ErrorLast { get; private set; }

        public QueryTraceLogger(ILogger<QueryTraceLogger> logger)
        {
            Logger = logger;
        }
        public void Trace(string query)
        {
            Logger.LogDebug(query);
            LastStatement = query;
        }

        public void Increment()
        {
            QueryCount++;
        }

        public Action<Exception> Mark()
        {
            Stopwatch sw = Stopwatch.StartNew();
            return e => 
            {
                sw.Stop();
                if(e==null)
                {
                    SuccessTimeLast = sw.ElapsedMilliseconds;
                    SuccessTimeAvg = (SuccessTimeAvg * SuccessCount + SuccessTimeLast) / (SuccessCount + 1);
                    SuccessCount++;
                }
                else
                {
                    ErrorLast = e;
                    ErrorTimeLast = sw.ElapsedMilliseconds;
                    ErrorTimeAvg = (ErrorTimeAvg * ErrorCount + ErrorTimeLast) / (ErrorCount + 1);
                    ErrorCount++;
                }
            };
        }
    }
}
