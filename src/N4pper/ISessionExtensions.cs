using System;
using System.Collections.Concurrent;
using AsIKnow.Graph;
using Neo4j.Driver;
using Neo4j.Driver.V1;

namespace N4pper
{
    public static class ISessionExtensions
    {
        public static ISession WithGraphManager(this ISession ext, GraphManager manager)
        {
            return new GraphManagedSession(ext, manager);
        }
    }
}
