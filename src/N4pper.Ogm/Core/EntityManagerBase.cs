using N4pper.Ogm.Entities;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Ogm.Core
{
    public abstract class EntityManagerBase
    {
        public IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            return CreateNodes(runner, entities.Select(p => new Tuple<IOgmEntity, IEnumerable<string>>(p, new string[0])));
        }
        public abstract IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities);
        public IEnumerable<IOgmEntity> UpdateNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            return UpdateNodes(runner, entities.Select(p => new Tuple<IOgmEntity, IEnumerable<string>>(p, new string[0])));
        }
        public abstract IEnumerable<IOgmEntity> UpdateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities);
        public abstract void DeleteNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities);
        public IEnumerable<IOgmConnection> CreateRels(IStatementRunner runner, IEnumerable<IOgmConnection> entities)
        {
            return CreateRels(runner, entities.Select(p => new Tuple<IOgmConnection, IEnumerable<string>>(p, new string[0])));
        }
        public abstract IEnumerable<IOgmConnection> CreateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmConnection, IEnumerable<string>>> entities);
        public IEnumerable<IOgmConnection> UpdateRels(IStatementRunner runner, IEnumerable<IOgmConnection> entities)
        {
            return UpdateRels(runner, entities.Select(p => new Tuple<IOgmConnection, IEnumerable<string>>(p, new string[0])));
        }
        public abstract IEnumerable<IOgmConnection> UpdateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmConnection, IEnumerable<string>>> entities);
        public abstract void DeleteRels(IStatementRunner runner, IEnumerable<IOgmConnection> entities);

        public abstract IEnumerable<IOgmConnection> MergeConnections(IStatementRunner runner, IEnumerable<Tuple<IOgmConnection, IEnumerable<string>>> entities);
    }
}
