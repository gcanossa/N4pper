using N4pper.Ogm.Entities;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Ogm
{
    public abstract class EntityManagerBase
    {
        public abstract IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities);
        public abstract IEnumerable<IOgmEntity> UpdateNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities);
        public abstract void DeleteNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities);
        public abstract IEnumerable<IOgmEntity> CreateRels(IStatementRunner runner, IEnumerable<Tuple<long, IOgmEntity, long>> entities);
        public abstract IEnumerable<IOgmEntity> UpdateRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities);
        public abstract void DeleteRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities);
    }
}
