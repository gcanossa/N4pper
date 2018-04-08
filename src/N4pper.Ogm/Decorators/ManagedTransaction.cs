using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using N4pper.Decorators;
using N4pper.Diagnostic;
using N4pper.Ogm;
using Neo4j.Driver.V1;

namespace N4pper.Ogm.Decorators
{
    public class ManagedTransaction : TransactionDecorator, IOgmStatementRunner
    {
        public GraphContext Context { get; internal set; }

        public ManagedTransaction(ITransaction transaction, N4pperManager manager, IGraphManagedStatementRunner parent) : base(transaction, manager, parent)
        {
        }
        public override void Dispose()
        {
            base.Dispose();
            Context.Dispose();
        }
    }
}
