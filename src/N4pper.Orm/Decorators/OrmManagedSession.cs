﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using N4pper.Diagnostic;
using N4pper.Orm;
using Neo4j.Driver.V1;

namespace N4pper.Decorators
{
    public class OrmManagedSession : SessionDecorator, IOrmStatementRunner
    {
        public GraphContext Context { get; internal set; }
        public OrmManagedSession(ISession session, N4pperManager manager) : base(session, manager)
        {
        }

        public override ITransaction BeginTransaction()
        {
            return new OrmManagedTransaction(base.BeginTransaction(), Manager) { Context = Context };
        }
        public override ITransaction BeginTransaction(string bookmark)
        {
            return new OrmManagedTransaction(base.BeginTransaction(bookmark), Manager) { Context = Context };
        }
        public override Task<ITransaction> BeginTransactionAsync()
        {
            return Task.Run<ITransaction>(async () => new OrmManagedTransaction(await base.BeginTransactionAsync(), Manager) { Context = Context });
        }
        public override void ReadTransaction(Action<ITransaction> work)
        {
            base.ReadTransaction(p=>work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }
        public override T ReadTransaction<T>(Func<ITransaction, T> work)
        {
            return base.ReadTransaction(p => work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }
        public override Task ReadTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.ReadTransactionAsync(p => work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }
        public override Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.ReadTransactionAsync(p => work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }
        public override void WriteTransaction(Action<ITransaction> work)
        {
            base.WriteTransaction(p => work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }
        public override T WriteTransaction<T>(Func<ITransaction, T> work)
        {
            return base.WriteTransaction(p => work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }
        public override Task WriteTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.WriteTransactionAsync(p => work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }
        public override Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.WriteTransactionAsync(p => work(new OrmManagedTransaction(p, Manager) { Context = Context }));
        }

        public override void Dispose()
        {
            base.Dispose();
            Context.Dispose();
        }
    }
}
