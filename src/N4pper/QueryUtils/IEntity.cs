using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public interface IEntity
    {
        Dictionary<string, object> Props { get; }

        Parameters Parametrize(string suffix = null);
        IEntity Parametrize(Parameters p);
    }
}
