using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Diagnostic
{
    public interface IQueryTracer
    {
        void Trace(string query);
    }
}
