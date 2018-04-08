using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using N4pper.Diagnostic;
using Neo4j.Driver.V1;

namespace N4pper.Decorators
{
    public class StatementRunnerDecorator : StatementRunnerDecoratorBase, IGraphManagedStatementRunner
    {
        public N4pperManager Manager { get; protected set; }
        public bool IsApocAvailable { get; protected set; }
        public StatementRunnerDecorator(IStatementRunner runner, N4pperManager manager, IGraphManagedStatementRunner parent) : base(runner)
        {
            Manager = manager;
            IsApocAvailable = parent.IsApocAvailable;
        }
        public override IStatementResult Run(Statement statement)
        {
            return Manager.ProfileQuery(statement.Text, () => base.Run(statement));
        }
        public override IStatementResult Run(string statement)
        {
            return Manager.ProfileQuery(statement, () => base.Run(statement));
        }
        public override IStatementResult Run(string statement, IDictionary<string, object> parameters)
        {
            return Manager.ProfileQuery(statement, () => base.Run(statement, parameters));
        }
        public override IStatementResult Run(string statement, object parameters)
        {
            return Manager.ProfileQuery(statement, () => base.Run(statement, parameters));
        }
        public override Task<IStatementResultCursor> RunAsync(Statement statement)
        {
            return Manager.ProfileQueryAsync(statement.Text, () => base.RunAsync(statement));
        }
        public override Task<IStatementResultCursor> RunAsync(string statement)
        {
            return Manager.ProfileQueryAsync(statement, () => base.RunAsync(statement));
        }
        public override Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters)
        {
            return Manager.ProfileQueryAsync(statement, () => base.RunAsync(statement, parameters));
        }
        public override Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        {
            return Manager.ProfileQueryAsync(statement, () => base.RunAsync(statement, parameters));
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        protected override IDictionary<string, object> FixParameters(IDictionary<string, object> param)
        {
            return Manager.ParamentersMangler.Mangle(param);
        }
    }
}
