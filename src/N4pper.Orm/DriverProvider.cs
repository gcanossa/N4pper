using N4pper.Decorators;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm
{
    public abstract class DriverProvider
    {
        public abstract string Uri { get; }
        public abstract IAuthToken AuthToken { get; }
        public abstract Config Config { get; }

        protected N4pperManager Manager { get; set; }

        public DriverProvider(N4pperManager manager)
        {
            Manager = manager;
        }

        public virtual IDriver GetDriver()
        {
            return new OrmManagedDriver(GraphDatabase.Driver(Uri, AuthToken, Config), Manager);
        }
    }

    public abstract class DriverProvider<T> : DriverProvider where T : GraphContext
    {
        public DriverProvider(N4pperManager manager) : base(manager)
        {
        }
    }
}
