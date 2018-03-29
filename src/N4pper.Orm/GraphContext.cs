using N4pper.Decorators;
using N4pper.Orm.Design;
using N4pper.Orm.Entities;
using N4pper.Orm.Queryable;
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

namespace N4pper.Orm
{
    public abstract class GraphContext : GraphContextBase
    {
        public IDriver Driver { get; protected set; }
        
        public GraphContext(DriverProvider provider)
            :base()
        {
            Driver = new OrmManagedDriver(provider.GetDriver(), provider.Manager, this);

            OnModelCreating(new GraphModelBuilder());

            Runner = Driver.Session();
        }
        
        public TransactionGraphContext GetTransactionContext()
        {
            return new TransactionGraphContext(((ISession)Runner).BeginTransaction());
        }

        protected virtual void OnModelCreating(GraphModelBuilder builder)
        {
            OrmCoreTypes.Entity<Entities.Connection>(p => new { p.SourcePropertyName, p.DestinationPropertyName, p.Version });
        }
    }
}
