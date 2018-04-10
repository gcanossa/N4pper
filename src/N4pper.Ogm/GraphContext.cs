using N4pper.Decorators;
using N4pper.Ogm.Core;
using N4pper.Ogm.Decorators;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using N4pper.Ogm.Queryable;
using N4pper.Queryable;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm
{
    public abstract class GraphContext : GraphContextBase
    {
        public IDriver Driver { get; protected set; }
        
        public GraphContext(DriverProvider provider, TypesManager typesManager, ChangeTrackerBase changeTracker, EntityManagerBase entityManager)
            :base(typesManager, changeTracker, entityManager)
        {
            Driver = new ManagedDriver(provider.GetDriver(), provider.Manager, this);

            OnModelCreating(new GraphModelBuilder(typesManager));

            Runner = Driver.Session();
        }
        
        public TransactionGraphContext GetTransactionContext()
        {
            return new TransactionGraphContext(((ISession)Runner).BeginTransaction(), TypesManager, ChangeTracker, EntityManager);
        }

        protected virtual void OnModelCreating(GraphModelBuilder builder)
        {
        }
    }
}
