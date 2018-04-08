using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;

namespace N4pper.Ogm.Core
{
    public class CypherEntityManager : EntityManagerBase
    {
        public override IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            List<IGrouping<Type,Tuple<int, IOgmEntity>>> sets = entities
                .Select((p, i) => new Tuple<int, IOgmEntity>(i, p))
                .GroupBy(p=>p.Item2.GetType()).ToList();

            List<Tuple<int, IOgmEntity>> results = new List<Tuple<int, IOgmEntity>>();

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                foreach (var set in sets)
                {
                    List<Tuple<int, IOgmEntity>> items = set.ToList();
                    Symbol row = new Symbol();
                    Symbol m = new Symbol();

                    StringBuilder sb = new StringBuilder();

                    sb.Append($"UNWIND $batch AS {row} ");
                    sb.Append($"CREATE {new Node(m, set.Key)} ");
                    sb.Append($"SET {m}+={row}.{nameof(NodeEntity.Properties)},{m}.{nameof(IOgmEntity.EntityId)}=id({m}) ");
                    sb.Append($"RETURN {m}");

                    results.AddRange(runner
                        .ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = set.Select(p => new NodeEntity(p.Item2, false).ToPropDictionary()).ToList() })
                        .ToList()
                        .Select((p,i)=>new Tuple<int, IOgmEntity>(items[i].Item1, p)));
                }

                return results.OrderBy(p => p.Item1).Select(p => p.Item2).ToList();
            }
        }

        public override IEnumerable<IOgmEntity> CreateRels(IStatementRunner runner, IEnumerable<Tuple<long, IOgmEntity, long>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p != null && p.Item2 != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            List<IGrouping<Type, Tuple<int, Tuple<long, IOgmEntity, long>>>> sets = entities
                .Select((p, i) => new Tuple<int, Tuple<long, IOgmEntity, long>>(i, p))
                .GroupBy(p => p.Item2.Item2.GetType()).ToList();

            List<Tuple<int, IOgmEntity>> results = new List<Tuple<int, IOgmEntity>>();

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                foreach (var set in sets)
                {
                    List<Tuple<int, Tuple<long, IOgmEntity, long>>> items = set.ToList();

                    Symbol row = new Symbol();
                    Symbol m = new Symbol();
                    Symbol s = new Symbol();
                    Symbol d = new Symbol();

                    StringBuilder sb = new StringBuilder();

                    sb.Append($"UNWIND $batch AS {row} ");
                    sb.Append($"MATCH ({s} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.SourceId)}}}) ");
                    sb.Append($"MATCH ({d} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.DestinationId)}}}) ");
                    sb.Append($"CREATE ({s})-{new Rel(m, set.Key)}->({d}) ");
                    sb.Append($"SET {m}+={row}.{nameof(RelEntity.Properties)},{m}.{nameof(IOgmEntity.EntityId)}=id({m}) ");
                    sb.Append($"RETURN {m}");

                    results.AddRange(runner
                        .ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = entities.Select(p => new RelEntity(p.Item2, p.Item1, p.Item3, false).ToPropDictionary()).ToList() })
                        .ToList()
                        .Select((p, i) => new Tuple<int, IOgmEntity>(items[i].Item1, p)));
                }

                return results.OrderBy(p => p.Item1).Select(p => p.Item2).ToList();
            }
        }

        public override void DeleteNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p != null && p.EntityId != null);

            if (entities.Count() == 0)
                return;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"MATCH ({m} {{{nameof(IOgmEntity.EntityId)}:{row}}})");
                sb.Append($"DETACH DELETE {m}");

                runner.Execute(sb.ToString(), new { batch = entities.Select(p => p.EntityId).ToList() });
            }
        }

        public override void DeleteRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p != null && p.EntityId != null);

            if (entities.Count() == 0)
                return;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"MATCH ()-[{m} {{{nameof(IOgmEntity.EntityId)}:{row}}}]->()");
                sb.Append($"DELETE {m}");

                runner.Execute(sb.ToString(), new { batch = entities.Select(p => p.EntityId).ToList() });
            }
        }

        public override IEnumerable<IOgmEntity> UpdateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p != null && p.Item1.EntityId != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"MATCH ({m} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(NodeEntity.Properties)}.{nameof(IOgmEntity.EntityId)}}})");
                sb.Append($"SET {m}+={row}.{nameof(NodeEntity.Properties)} ");
                sb.Append($"RETURN {m}");

                return runner.ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = entities.Select(p => new NodeEntity(p.Item1).ToPropDictionary().ExludeProperties(p.Item2 ?? new string[0])).ToList() }).ToList();
            }
        }

        public override IEnumerable<IOgmEntity> UpdateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p != null && p.Item1.EntityId != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"MATCH ()-[{m} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.Properties)}.{nameof(IOgmEntity.EntityId)}}}]->()");
                sb.Append($"SET {m}+={row}.{nameof(RelEntity.Properties)} ");
                sb.Append($"RETURN {m}");

                return runner.ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = entities.Select(p => new RelEntity(p.Item1, -1, -1).ToPropDictionary().ExludeProperties(p.Item2 ?? new string[0])).ToList() }).ToList();
            }
        }
    }
}
