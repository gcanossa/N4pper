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
    public class ManagedSession : SessionDecorator, IOgmStatementRunner
    {
        public GraphContext Context { get; internal set; }
        public ManagedSession(ISession session, N4pperManager manager, IGraphManagedStatementRunner parent) : base(session, manager, parent)
        {
        }

        public override ITransaction BeginTransaction()
        {
            return new ManagedTransaction(base.BeginTransaction(), Manager, this) { Context = Context };
        }
        public override ITransaction BeginTransaction(string bookmark)
        {
            return new ManagedTransaction(base.BeginTransaction(bookmark), Manager, this) { Context = Context };
        }
        public override Task<ITransaction> BeginTransactionAsync()
        {
            return Task.Run<ITransaction>(async () => new ManagedTransaction(await base.BeginTransactionAsync(), Manager, this) { Context = Context });
        }
        public override void ReadTransaction(Action<ITransaction> work)
        {
            base.ReadTransaction(p=>work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }
        public override T ReadTransaction<T>(Func<ITransaction, T> work)
        {
            return base.ReadTransaction(p => work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }
        public override Task ReadTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.ReadTransactionAsync(p => work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }
        public override Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.ReadTransactionAsync(p => work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }
        public override void WriteTransaction(Action<ITransaction> work)
        {
            base.WriteTransaction(p => work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }
        public override T WriteTransaction<T>(Func<ITransaction, T> work)
        {
            return base.WriteTransaction(p => work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }
        public override Task WriteTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.WriteTransactionAsync(p => work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }
        public override Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.WriteTransactionAsync(p => work(new ManagedTransaction(p, Manager, this) { Context = Context }));
        }

        public override void Dispose()
        {
            base.Dispose();
            Context.Dispose();
        }
    }
}
