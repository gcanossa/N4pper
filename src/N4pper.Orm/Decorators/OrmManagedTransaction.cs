using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using N4pper.Diagnostic;
using N4pper.Orm;
using Neo4j.Driver.V1;

namespace N4pper.Decorators
{
    public class OrmManagedTransaction : TransactionDecorator, IOrmStatementRunner
    {
        public GraphContext Context { get; internal set; }

        public OrmManagedTransaction(ITransaction transaction, N4pperManager manager) : base(transaction, manager)
        {
        }
        public override void Dispose()
        {
            base.Dispose();
            Context.Dispose();
        }
    }
}
