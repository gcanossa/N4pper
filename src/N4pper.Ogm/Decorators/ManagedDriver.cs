using N4pper.Decorators;
using N4pper.Ogm;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm.Decorators
{
    public class ManagedDriver : DriverDecorator, IOgmStatementRunner
    {
        public GraphContext Context { get; protected set; }
        public ManagedDriver(IDriver driver, N4pperManager manager, GraphContext context) : base(driver, manager)
        {
            Manager = manager;
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override ISession Session()
        {
            return base.Session().WithGraphManager(Manager, this, Context);
        }
        
        public override ISession Session(AccessMode defaultMode)
        {
            return base.Session(defaultMode).WithGraphManager(Manager, this, Context);
        }

        public override ISession Session(string bookmark)
        {
            return base.Session(bookmark).WithGraphManager(Manager, this, Context);
        }

        public override ISession Session(AccessMode defaultMode, string bookmark)
        {
            return base.Session(defaultMode, bookmark).WithGraphManager(Manager, this, Context);
        }

        public override ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return base.Session(defaultMode, bookmarks).WithGraphManager(Manager, this, Context);
        }

        public override ISession Session(IEnumerable<string> bookmarks)
        {
            return base.Session(bookmarks).WithGraphManager(Manager, this, Context);
        }
    }
}
