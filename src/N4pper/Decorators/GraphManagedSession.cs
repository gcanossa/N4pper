using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AsIKnow.Graph;
using N4pper.Diagnostic;
using Neo4j.Driver.V1;

namespace N4pper.Decorators
{
    public class GraphManagedSession : SessionDecorator, IGraphManagedStatementRunner
    {
        public GraphManager Manager { get; protected set; }
        public N4pperOptions Options { get; protected set; }
        public IQueryTracer Tracer { get; protected set; }
        public GraphManagedSession(ISession session, GraphManager manager, N4pperOptions options, IQueryTracer tracer) : base(session)
        {
            Manager = manager;
            Options = options;
            Tracer = tracer;
        }

        public override ITransaction BeginTransaction()
        {
            return new GraphManagedTransaction(base.BeginTransaction(), Manager, Options, Tracer);
        }
        public override ITransaction BeginTransaction(string bookmark)
        {
            return new GraphManagedTransaction(base.BeginTransaction(bookmark), Manager, Options, Tracer);
        }
        public override Task<ITransaction> BeginTransactionAsync()
        {
            return Task.Run<ITransaction>(async () => new GraphManagedTransaction(await base.BeginTransactionAsync(), Manager, Options, Tracer));
        }
        public override void ReadTransaction(Action<ITransaction> work)
        {
            base.ReadTransaction(p=>work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
        }
        public override T ReadTransaction<T>(Func<ITransaction, T> work)
        {
            return base.ReadTransaction(p => work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
        }
        public override Task ReadTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.ReadTransactionAsync(p => work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
        }
        public override Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.ReadTransactionAsync(p => work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
        }
        public override void WriteTransaction(Action<ITransaction> work)
        {
            base.WriteTransaction(p => work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
        }
        public override T WriteTransaction<T>(Func<ITransaction, T> work)
        {
            return base.WriteTransaction(p => work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
        }
        public override Task WriteTransactionAsync(Func<ITransaction, Task> work)
        {
            return base.WriteTransactionAsync(p => work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
        }
        public override Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return base.WriteTransactionAsync(p => work(new GraphManagedTransaction(p, Manager, Options, Tracer)));
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
