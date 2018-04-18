using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public interface IQueryParamentersMangler
    {
        IDictionary<string, object> Mangle(object param);
    }
}
