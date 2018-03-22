using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Diagnostic
{
    public interface IQueryProfiler
    {
        void Increment();
        Action<Exception> Mark();
        void Trace(string query);
    }
}
