using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public interface IEntity
    {
        IDictionary<string, object> Props { get; }

        Parameters Parametrize(string suffix = null);
        IEntity Parametrize(Parameters p);
    }
}
