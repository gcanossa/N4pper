using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Diagnostic
{
    public interface IQueryProfiler
    {
        Action<Exception> Mark(string query);
    }
}
