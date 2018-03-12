﻿using N4pper.Orm;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Decorators
{
    public class GraphManagedDriver : DriverDecorator, IGraphManagedStatementRunner
    {
        public GraphContext Context { get; internal set; }
        public N4pperManager Manager { get; protected set; }
        public GraphManagedDriver(IDriver driver, N4pperManager manager) : base(driver)
        {
            Manager = manager;
        }

        public override ISession Session()
        {
            return base.Session().WithGraphManager(Manager, Context);
        }
        
        public override ISession Session(AccessMode defaultMode)
        {
            return base.Session(defaultMode).WithGraphManager(Manager, Context);
        }

        public override ISession Session(string bookmark)
        {
            return base.Session(bookmark).WithGraphManager(Manager, Context);
        }

        public override ISession Session(AccessMode defaultMode, string bookmark)
        {
            return base.Session(defaultMode, bookmark).WithGraphManager(Manager, Context);
        }

        public override ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return base.Session(defaultMode, bookmarks).WithGraphManager(Manager, Context);
        }

        public override ISession Session(IEnumerable<string> bookmarks)
        {
            return base.Session(bookmarks).WithGraphManager(Manager, Context);
        }
    }
}
