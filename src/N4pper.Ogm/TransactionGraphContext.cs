using System;
using System.Collections.Generic;
using System.Text;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using Neo4j.Driver.V1;

namespace N4pper.Ogm
{
    public sealed class TransactionGraphContext : GraphContextBase
    {
        public TransactionGraphContext(ITransaction runner, TypesManager typesManager, ChangeTrackerBase changeTracker, EntityManagerBase entityManager) 
            : base(runner, typesManager, changeTracker, entityManager)
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
