using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AsIKnow.Graph;
using Neo4j.Driver.V1;

namespace N4pper
{
    public class GraphManagedSession : SessionDecorator, IGraphManagedStatementRunner
    {
        public GraphManager Manager { get; protected set; }
        public GraphManagedSession(ISession session, GraphManager manager) : base(session)
        {
            Manager = manager;
        }

        public override ITransaction BeginTransaction()
        {
            return new GraphManagedTransaction(base.BeginTransaction(), Manager);
        }
        public override ITransaction BeginTransaction(string bookmark)
        {
            return new GraphManagedTransaction(base.BeginTransaction(bookmark), Manager);
        }
        public override Task<ITransaction> BeginTransactionAsync()
        {
            return Task.Run<ITransaction>(async () => new GraphManagedTransaction(await base.BeginTransactionAsync(), Manager));
        }
        public override void ReadTransaction(Action<ITransaction> work)
        {
            base.ReadTransaction(p=>work(new GraphManagedTransaction(p, Manager)));
        }
        public override T ReadTransaction<T>(Func<ITransaction, T> work)
        {
            return base.ReadTransaction(p => work(new GraphManagedTransaction(p, Manager)));
        }
        public override Task ReadTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.ReadTransactionAsync(p => work(new GraphManagedTransaction(p, Manager)));
        }
        public override Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.ReadTransactionAsync(p => work(new GraphManagedTransaction(p, Manager)));
        }
        public override void WriteTransaction(Action<ITransaction> work)
        {
            base.WriteTransaction(p => work(new GraphManagedTransaction(p, Manager)));
        }
        public override T WriteTransaction<T>(Func<ITransaction, T> work)
        {
            return base.WriteTransaction(p => work(new GraphManagedTransaction(p, Manager)));
        }
        public override Task WriteTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.WriteTransactionAsync(p => work(new GraphManagedTransaction(p, Manager)));
        }
        public override Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.WriteTransactionAsync(p => work(new GraphManagedTransaction(p, Manager)));
        }
    }
}
