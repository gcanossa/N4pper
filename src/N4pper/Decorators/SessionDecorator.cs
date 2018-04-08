using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using N4pper.Diagnostic;
using Neo4j.Driver.V1;

namespace N4pper.Decorators
{
    public class SessionDecorator : SessionDecoratorBase, IGraphManagedStatementRunner
    {
        public N4pperManager Manager { get; protected set; }
        public bool IsApocAvailable { get; protected set; }
        public SessionDecorator(ISession session, N4pperManager manager, IGraphManagedStatementRunner parent) : base(session)
        {
            Manager = manager;
            IsApocAvailable = parent.IsApocAvailable;
        }

        public override ITransaction BeginTransaction()
        {
            return new TransactionDecorator(base.BeginTransaction(), Manager, this);
        }
        public override ITransaction BeginTransaction(string bookmark)
        {
            return new TransactionDecorator(base.BeginTransaction(bookmark), Manager, this);
        }
        public override Task<ITransaction> BeginTransactionAsync()
        {
            return Task.Run<ITransaction>(async () => new TransactionDecorator(await base.BeginTransactionAsync(), Manager, this));
        }
        public override void ReadTransaction(Action<ITransaction> work)
        {
            base.ReadTransaction(p => work(new TransactionDecorator(p, Manager, this)));
        }
        public override T ReadTransaction<T>(Func<ITransaction, T> work)
        {
            return base.ReadTransaction(p => work(new TransactionDecorator(p, Manager, this)));
        }
        public override Task ReadTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.ReadTransactionAsync(p => work(new TransactionDecorator(p, Manager, this)));
        }
        public override Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.ReadTransactionAsync(p => work(new TransactionDecorator(p, Manager, this)));
        }
        public override void WriteTransaction(Action<ITransaction> work)
        {
            base.WriteTransaction(p => work(new TransactionDecorator(p, Manager, this)));
        }
        public override T WriteTransaction<T>(Func<ITransaction, T> work)
        {
            return base.WriteTransaction(p => work(new TransactionDecorator(p, Manager, this)));
        }
        public override Task WriteTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.WriteTransactionAsync(p => work(new TransactionDecorator(p, Manager, this)));
        }
        public override Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.WriteTransactionAsync(p => work(new TransactionDecorator(p, Manager, this)));
        }


        public override IStatementResult Run(Statement statement)
        {
            return Manager.ProfileQuery(statement.Text,() => base.Run(statement));
        }
        public override IStatementResult Run(string statement)
        {
            return Manager.ProfileQuery(statement,() => base.Run(statement));
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
