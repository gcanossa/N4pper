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
        public IEnumerable<IOgmEntity> CreateRels(IStatementRunner runner, IEnumerable<Tuple<long, IOgmEntity, long>> entities)
        {
            return CreateRels(runner, entities.Select(p => new Tuple<long, Tuple<IOgmEntity, IEnumerable<string>>, long>(p.Item1, new Tuple<IOgmEntity, IEnumerable<string>>(p.Item2, new string[0]), p.Item3)));
        }
        public abstract IEnumerable<IOgmEntity> CreateRels(IStatementRunner runner, IEnumerable<Tuple<long, Tuple<IOgmEntity, IEnumerable<string>>, long>> entities);
        public IEnumerable<IOgmEntity> UpdateRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            return UpdateRels(runner, entities.Select(p => new Tuple<IOgmEntity, IEnumerable<string>>(p, new string[0])));
        }
        public abstract IEnumerable<IOgmEntity> UpdateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities);
        public abstract void DeleteRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities);

        public abstract IEnumerable<Connection> MergeConnections(IStatementRunner runner, IEnumerable<Tuple<long, Tuple<Connection, IEnumerable<string>>, long>> entities);
    }
}
