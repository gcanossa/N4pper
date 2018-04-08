using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Decorators
{
    public class DriverDecorator : DriverDecoratorBase, IGraphManagedStatementRunner
    {
        public N4pperManager Manager { get; protected set; }
        public bool IsApocAvailable { get; protected set; }
        public DriverDecorator(IDriver driver, N4pperManager manager) : base(driver)
        {
            Manager = manager;

            using (ISession session = base.Session().WithGraphManager(Manager, this))
            {
                IStatementResult res = session.Run("CALL dbms.procedures() YIELD name WITH name WHERE name STARTS WITH 'apoc.' RETURN count(*)");
                IsApocAvailable = ((long)res.ToList().First().Values.First().Value) > 0;
            }
        }

        public override ISession Session()
        {
            return base.Session().WithGraphManager(Manager, this);
        }

        public override ISession Session(AccessMode defaultMode)
        {
            return base.Session(defaultMode).WithGraphManager(Manager, this);
        }

        public override ISession Session(string bookmark)
        {
            return base.Session(bookmark).WithGraphManager(Manager, this);
        }

        public override ISession Session(AccessMode defaultMode, string bookmark)
        {
            return base.Session(defaultMode, bookmark).WithGraphManager(Manager, this);
        }

        public override ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return base.Session(defaultMode, bookmarks).WithGraphManager(Manager, this);
        }

        public override ISession Session(IEnumerable<string> bookmarks)
        {
            return base.Session(bookmarks).WithGraphManager(Manager, this);
        }
    }
}
