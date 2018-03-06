using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace N4pper
{
    public class TransactionDecorator : StatementRunnerDecorator, ITransaction
    {
        protected ITransaction Transaction { get; set; }

        public TransactionDecorator(ITransaction transaction) : base(transaction)
        {
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        #region ITransaction

        public Task CommitAsync()
        {
            return Transaction.CommitAsync();
        }

        public void Failure()
        {
            Transaction.Failure();
        }

        public Task RollbackAsync()
        {
            return Transaction.RollbackAsync();
        }
        
        public void Success()
        {
            Transaction.Success();
        }

        #endregion
    }
}
