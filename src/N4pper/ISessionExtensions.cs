using System;
using System.Collections.Concurrent;
using Neo4j.Driver;
using Neo4j.Driver.V1;
using N4pper.Decorators;
using N4pper.Diagnostic;

namespace N4pper
{
    public static class ISessionExtensions
    {
        public static ISession WithGraphManager(this ISession ext, N4pperManager manager, IGraphManagedStatementRunner parent)
        {
            return new SessionDecorator(ext, manager, parent);
        }
    }
}
