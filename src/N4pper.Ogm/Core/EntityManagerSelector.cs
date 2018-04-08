using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using N4pper.Ogm.Entities;
using Neo4j.Driver.V1;

namespace N4pper.Ogm.Core
{
    public class EntityManagerSelector : EntityManagerBase
    {
        protected Dictionary<Func<IStatementRunner, bool>, EntityManagerBase> Managers { get; set; } = new Dictionary<Func<IStatementRunner, bool>, EntityManagerBase>();
        protected EntityManagerBase Default { get; set; }

        public EntityManagerSelector(EntityManagerBase defaultManager, IEnumerable<KeyValuePair<Func<IStatementRunner, bool>, EntityManagerBase>> managers = null)
        {
            Default = defaultManager ?? throw new ArgumentNullException(nameof(defaultManager));
            Managers = managers?.ToDictionary(p=>p.Key, p=>p.Value);
        }

        protected EntityManagerBase SelectManager(IStatementRunner runner)
        {
            if (Managers?.Any(p => p.Key(runner)) ?? false)
                return Managers.First(p => p.Key(runner)).Value;
            else
                return Default;
        }

        public override IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            return SelectManager(runner).CreateNodes(runner, entities);
        }

        public override IEnumerable<IOgmEntity> CreateRels(IStatementRunner runner, IEnumerable<Tuple<long, IOgmEntity, long>> entities)
        {
            return SelectManager(runner).CreateRels(runner, entities);
        }

        public override void DeleteNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            SelectManager(runner).DeleteNodes(runner, entities);
        }

        public override void DeleteRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            SelectManager(runner).DeleteRels(runner, entities);
        }

        public override IEnumerable<IOgmEntity> UpdateNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            return SelectManager(runner).UpdateNodes(runner, entities);
        }

        public override IEnumerable<IOgmEntity> UpdateRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            return SelectManager(runner).UpdateRels(runner, entities);
        }
    }
}
