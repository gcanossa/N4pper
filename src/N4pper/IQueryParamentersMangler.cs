using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public interface IQueryParamentersMangler
    {
        IDictionary<string, object> Mangle(IDictionary<string, object> param);
    }
}
