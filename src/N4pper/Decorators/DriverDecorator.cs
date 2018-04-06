using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Decorators
{
    public class DriverDecorator : DriverDecoratorBase, IGraphManagedStatementRunner
    {
        public N4pperManager Manager { get; protected set; }
        public DriverDecorator(IDriver driver, N4pperManager manager) : base(driver)
        {
            Manager = manager;
        }

        public override ISession Session()
        {
            return base.Session().WithGraphManager(Manager);
        }

        public override ISession Session(AccessMode defaultMode)
        {
            return base.Session(defaultMode).WithGraphManager(Manager);
        }

        public override ISession Session(string bookmark)
        {
            return base.Session(bookmark).WithGraphManager(Manager);
        }

        public override ISession Session(AccessMode defaultMode, string bookmark)
        {
            return base.Session(defaultMode, bookmark).WithGraphManager(Manager);
        }

        public override ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return base.Session(defaultMode, bookmarks).WithGraphManager(Manager);
        }

        public override ISession Session(IEnumerable<string> bookmarks)
        {
            return base.Session(bookmarks).WithGraphManager(Manager);
        }
    }
}
