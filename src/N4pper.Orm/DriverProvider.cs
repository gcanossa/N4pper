using N4pper.Decorators;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm
{
    public abstract class DriverProvider<T> : DriverProvider where T : GraphContext
    {
        public DriverProvider(N4pperManager manager) : base(manager)
        {
        }
    }
}
