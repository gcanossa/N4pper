using System;
using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;

namespace N4pper.Orm
{
    public sealed class TransactionGraphContext : GraphContextBase
    {
        public TransactionGraphContext(ITransaction runner) : base(runner)
        {
        }
        private ITransaction Transaction => (ITransaction)Runner;
        public void Commit()
        {
            Transaction.Success();
        }
        public void Rollback()
        {
            Transaction.Failure();
        }
    }
}
