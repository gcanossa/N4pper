using N4pper.Decorators;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm
{
    public abstract class GraphContext
    {
        public IDriver Driver { get; protected set; }
        public GraphContext(DriverProvider provider)
        {
            Driver = provider.GetDriver();
            ((OrmManagedDriver)Driver).Context = this;
            OnModelCreating(new GraphModelBuilder());
        }
        
        protected virtual void OnModelCreating(GraphModelBuilder builder)
        {

        }
    }
}
