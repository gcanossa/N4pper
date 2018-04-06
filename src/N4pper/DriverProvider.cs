using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public abstract class DriverProvider
    {
        public abstract string Uri { get; }
        public abstract IAuthToken AuthToken { get; }
        public abstract Config Config { get; }

        public N4pperManager Manager { get; protected set; }

        public DriverProvider(N4pperManager manager)
        {
            Manager = manager;
        }

        private IDriver Instance { get; set; }

        public virtual IDriver GetDriver()
        {
            if (Instance == null)
                Instance = GraphDatabase.Driver(Uri, AuthToken, Config);
            try
            {
                Instance.Session().Dispose();
            }
            catch
            {
                Instance = GraphDatabase.Driver(Uri, AuthToken, Config);
            }

            return Instance;
        }
    }
}
