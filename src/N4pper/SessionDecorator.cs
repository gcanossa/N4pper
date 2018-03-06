using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace N4pper
{
    public class SessionDecorator : StatementRunnerDecorator, ISession
    {
        protected ISession Session { get; set; }
        
        public SessionDecorator(ISession session) : base(session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        #region ISession

        public virtual string LastBookmark => Session.LastBookmark;

        public virtual ITransaction BeginTransaction()
        {
            return Session.BeginTransaction();
        }

        public virtual ITransaction BeginTransaction(string bookmark)
        {
            return Session.BeginTransaction(bookmark);
        }

        public virtual Task<ITransaction> BeginTransactionAsync()
        {
            return Session.BeginTransactionAsync();
        }

        public virtual T ReadTransaction<T>(Func<ITransaction, T> work)
        {
            return Session.ReadTransaction<T>(work);
        }

        public virtual Task<T> ReadTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return Session.ReadTransactionAsync<T>(work);
        }

        public virtual void ReadTransaction(Action<ITransaction> work)
        {
            Session.ReadTransaction(work);
        }

        public virtual Task ReadTransactionAsync(Func<ITransaction, Task> work)
        {
            return Session.ReadTransactionAsync(work);
        }

        public virtual T WriteTransaction<T>(Func<ITransaction, T> work)
        {
            return Session.WriteTransaction<T>(work);
        }

        public virtual Task<T> WriteTransactionAsync<T>(Func<ITransaction, Task<T>> work)
        {
            return Session.WriteTransactionAsync<T>(work);
        }

        public virtual void WriteTransaction(Action<ITransaction> work)
        {
            Session.WriteTransaction(work);
        }

        public virtual Task WriteTransactionAsync(Func<ITransaction, Task> work)
        {
            return Session.WriteTransactionAsync(work);
        }

        public virtual Task CloseAsync()
        {
            return Session.CloseAsync();
        }

        #endregion
    }
}
