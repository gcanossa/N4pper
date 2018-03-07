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
        public GraphManager Manager { get; protected set; }
        public N4pperOptions Options { get; protected set; }
        public IQueryTracer Tracer { get; protected set; }
        public GraphManagedStatementRunner(IStatementRunner runner, GraphManager manager, N4pperOptions options, IQueryTracer tracer) : base(runner)
        {
            Manager = manager;
            Options = options;
            Tracer = tracer;
        }

        public override IStatementResult Run(Statement statement)
        {
            Tracer?.Trace(statement.Text);
            return base.Run(statement);
        }
        public override IStatementResult Run(string statement)
        {
            Tracer?.Trace(statement);
            return base.Run(statement);
        }
        public override IStatementResult Run(string statement, IDictionary<string, object> parameters)
        {
            Tracer?.Trace(statement);
            return base.Run(statement, parameters);
        }
        public override IStatementResult Run(string statement, object parameters)
        {
            Tracer?.Trace(statement);
            return base.Run(statement, parameters);
        }
        public override Task<IStatementResultCursor> RunAsync(Statement statement)
        {
            Tracer?.Trace(statement.Text);
            return base.RunAsync(statement);
        }
        public override Task<IStatementResultCursor> RunAsync(string statement)
        {
            Tracer?.Trace(statement);
            return base.RunAsync(statement);
        }
        public override Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters)
        {
            Tracer?.Trace(statement);
            return base.RunAsync(statement, parameters);
        }
        public override Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        {
            Tracer?.Trace(statement);
            return base.RunAsync(statement, parameters);
        }
    }
}
