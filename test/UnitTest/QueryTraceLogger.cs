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
        public static string LastStatement { get; private set; }

        public static int QueryCount { get; private set; } = 0;
        public static int SuccessCount { get; private set; } = 0;
        public static double SuccessTimeAvg { get; private set; } = 0;
        public static double SuccessTimeLast { get; private set; } = 0;
        public static int ErrorCount { get; private set; } = 0;
        public static double ErrorTimeAvg { get; private set; } = 0;
        public static double ErrorTimeLast { get; private set; } = 0;
        public static Exception ErrorLast { get; private set; }

        public QueryTraceLogger()
        {
        }

        public Action<Exception> Mark(string query)
        {
            QueryCount++;
            LastStatement = query;
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
