using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace N4pper.Decorators
{
    public class StatementRunnerDecorator : IStatementRunner
    {
        public IStatementRunner Runner { get; protected set; }

        public StatementRunnerDecorator(IStatementRunner runner)
        {
            Runner = runner ?? throw new ArgumentNullException(nameof(runner));
        }

        #region IStatementRunner
        
        public virtual IStatementResult Run(string statement)
        {
            return Runner.Run(statement);
        }

        public virtual Task<IStatementResultCursor> RunAsync(string statement)
        {
            return Runner.RunAsync(statement);
        }

        public virtual IStatementResult Run(string statement, object parameters)
        {
            return Runner.Run(statement, parameters);
        }

        public virtual Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        {
            return Runner.RunAsync(statement, parameters);
        }

        public virtual IStatementResult Run(string statement, IDictionary<string, object> parameters)
        {
            return Runner.Run(statement, parameters);
        }

        public virtual Task<IStatementResultCursor> RunAsync(string statement, IDictionary<string, object> parameters)
        {
            return Runner.RunAsync(statement, parameters);
        }

        public virtual IStatementResult Run(Statement statement)
        {
            return Runner.Run(statement);
        }

        public virtual Task<IStatementResultCursor> RunAsync(Statement statement)
        {
            return Runner.RunAsync(statement);
        }

        public virtual void Dispose()
        {
            Runner.Dispose();
        }

        #endregion
    }
}
