using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using N4pper.Diagnostic;
using N4pper.Orm;
using Neo4j.Driver.V1;

namespace N4pper.Decorators
{
    public class GraphManagedTransaction : TransactionDecorator, IGraphManagedStatementRunner
    {
        public GraphContext Context { get; internal set; }
        public N4pperManager Manager { get; protected set; }

        public GraphManagedTransaction(ITransaction transaction, N4pperManager manager) : base(transaction)
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
