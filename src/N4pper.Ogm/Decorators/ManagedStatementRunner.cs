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
    public class ManagedStatementRunner : StatementRunnerDecorator, IOgmStatementRunner
    {
        public GraphContext Context { get; internal set; }
        public ManagedStatementRunner(IStatementRunner runner, N4pperManager manager, IGraphManagedStatementRunner parent) : base(runner, manager, parent)
        {
        }
        public override void Dispose()
        {
            base.Dispose();
            Context.Dispose();
        }
    }
}
