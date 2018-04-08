using System;
using System.Collections.Concurrent;
using Neo4j.Driver;
using Neo4j.Driver.V1;
using N4pper.Decorators;
using N4pper.Diagnostic;
using N4pper.Ogm;
using N4pper.Ogm.Decorators;

namespace N4pper.Ogm
{
    public static class ISessionExtensions
    {
        public static ISession WithGraphManager(this ISession ext, N4pperManager manager, IGraphManagedStatementRunner parent, GraphContext context)
        {
            return new ManagedSession(ext, manager, parent) { Context = context};
        }
    }
}
