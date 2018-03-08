using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AsIKnow.Graph;
using N4pper.Diagnostic;
using Neo4j.Driver.V1;

namespace N4pper.Decorators
{
    public class GraphManagedStatementRunner : StatementRunnerDecorator, IGraphManagedStatementRunner
    {
        public N4pperManager Manager { get; protected set; }
        public GraphManagedStatementRunner(IStatementRunner runner, N4pperManager manager) : base(runner)
        {
            Manager = manager;
        }

        public override IStatementResult Run(Statement statement)
        {
            Manager.TraceStatement(statement.Text);
            return base.Run(statement);
        }
        public override IStatementResult Run(string statement)
        {
            Manager.TraceStatement(statement);
            return base.Run(statement);
        }
        public override IStatementResult Run(string statement, IDictionary<string, object> parameters)
        {
            Manager.TraceStatement(statement);
            return base.Run(statement, parameters);
        }
        public override IStatementResult Run(string statement, object parameters)
        {
            Manager.TraceStatement(statement);
            return base.Run(statement, parameters);
        }
        public override Task<IStatementResultCursor> RunAsync(Statement statement)
        {
            Manager.TraceStatement(statement.Text);
            return base.RunAsync(statement);
        }
        public override Task<IStatementResultCursor> RunAsync(string statement)
        {
            Manager.TraceStatement(statement);
            return base.RunAsync(statement);
        }
        public override Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters)
        {
            Manager.TraceStatement(statement);
            return base.RunAsync(statement, parameters);
        }
        public override Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        {
            Manager.TraceStatement(statement);
            return base.RunAsync(statement, parameters);
        }
    }
}
