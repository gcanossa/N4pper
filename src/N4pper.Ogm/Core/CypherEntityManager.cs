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
        public override IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.Item1 != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            List<IGrouping<Type,Tuple<int, Type, NodeEntity>>> sets = entities
                .Select((p, i) => new Tuple<int, Type, NodeEntity>(i, p.Item1.GetType(), new NodeEntity(p.Item1, false, p.Item2)))
                .GroupBy(p=>p.Item2).ToList();

            List<Tuple<int, IOgmEntity>> results = new List<Tuple<int, IOgmEntity>>();

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                foreach (var set in sets)
                {
                    List<Tuple<int, Type, NodeEntity>> items = set.ToList();
                    Symbol row = new Symbol();
                    Symbol m = new Symbol();

                    StringBuilder sb = new StringBuilder();

                    sb.Append($"UNWIND $batch AS {row} ");
                    sb.Append($"CREATE {new Node(m, set.Key)} ");
                    sb.Append($"SET {m}+={row}.{nameof(NodeEntity.Properties)},{m}.{nameof(IOgmEntity.EntityId)}=id({m}) ");
                    sb.Append($"RETURN {m}");

                    results.AddRange(runner
                        .ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = set.Select(p => p.Item3.ToPropDictionary()).ToList() })
                        .ToList()
                        .Select((p,i)=>new Tuple<int, IOgmEntity>(items[i].Item1, p)));
                }

                return results.OrderBy(p => p.Item1).Select(p => p.Item2).ToList();
            }
        }

        public override IEnumerable<IOgmConnection> CreateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmConnection, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.Item1 != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            List<IGrouping<Type, Tuple<int, Type, RelEntity>>> sets = entities
                .Select((p, i) => 
                new Tuple<int, Type, RelEntity>(
                    i, 
                    p.Item1.GetType(),
                    new RelEntity(p.Item1, p.Item1.Source.EntityId.Value, p.Item1.Destination.EntityId.Value, false, p.Item2)))
                .GroupBy(p => p.Item2).ToList();

            List<Tuple<int, IOgmEntity>> results = new List<Tuple<int, IOgmEntity>>();

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                foreach (var set in sets)
                {
                    List<Tuple<int, Type, RelEntity>> items = set.ToList();

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
                        .ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = items.Select(p => p.Item3.ToPropDictionary()).ToList() })
                        .ToList()
                        .Select((p, i) => new Tuple<int, IOgmEntity>(items[i].Item1, p)));
                }

                return results.OrderBy(p => p.Item1).Select(p => p.Item2 as IOgmConnection).ToList();
            }
        }

        public override void DeleteNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.EntityId != null);

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

        public override void DeleteRels(IStatementRunner runner, IEnumerable<IOgmConnection> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.EntityId != null);

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

        public override IEnumerable<IOgmConnection> MergeConnections(IStatementRunner runner, IEnumerable<Tuple<IOgmConnection, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.Item1 != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                List<IGrouping<string, Tuple<int,ConnectionEntity>>> sets = entities
                .Select((p, i) => new Tuple<int, ConnectionEntity>(i, new ConnectionEntity(p.Item1, p.Item1.Source.EntityId.Value, p.Item1.Destination.EntityId.Value, version, excludePorperties: p.Item2)))
                .GroupBy(p => p.Item2.Label).ToList();

                Symbol row = new Symbol();
                Symbol s = new Symbol();
                Symbol d = new Symbol();
                Symbol r = new Symbol();

                List<Tuple<int, IOgmEntity>> results = new List<Tuple<int, IOgmEntity>>();

                foreach (var set in sets)
                {
                    List<Tuple<int, ConnectionEntity>> items = set.ToList();

                    StringBuilder sb = new StringBuilder();

                    sb.Append($"UNWIND $batch AS {row} ");
                    sb.Append($"MATCH ({s} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.SourceId)}}}) ");
                    sb.Append($"MATCH ({d} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.DestinationId)}}}) ");
                    sb.Append($"MERGE ({s})-");
                    sb.Append($"[{r}:`{set.Key}` {{" +
                        $"{nameof(OgmConnection.SourcePropertyName)}:{row}.{nameof(RelEntity.Properties)}.{nameof(OgmConnection.SourcePropertyName)}," +
                        $"{nameof(OgmConnection.DestinationPropertyName)}:{row}.{nameof(RelEntity.Properties)}.{nameof(OgmConnection.DestinationPropertyName)}," +
                        $"{nameof(OgmConnection.Order)}:{row}.{nameof(RelEntity.Properties)}.{nameof(OgmConnection.Order)}" +
                        $"}}]");
                    sb.Append($"->({d}) ");
                    sb.Append($"ON CREATE SET {r}+={row}.{nameof(RelEntity.Properties)},{r}.{nameof(IOgmEntity.EntityId)}=id({r}) ");
                    sb.Append($"ON MATCH SET {r}+={row}.{nameof(RelEntity.Properties)},{r}.{nameof(IOgmEntity.EntityId)}=id({r}) ");
                    sb.Append($"RETURN {r}");

                    object batch = new { batch = set.Select(p => p.Item2.ToPropDictionary()).ToList() };

                    results.AddRange(runner
                        .ExecuteQuery<IOgmEntity>(sb.ToString(), batch).ToList()
                        .Select((p, i) => new Tuple<int, IOgmEntity>(items[i].Item1, p)));

                    sb.Clear();

                    sb.Append($"UNWIND $batch AS {row} ");
                    sb.Append($"MATCH ({s} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.SourceId)}}}) ");
                    sb.Append($"MATCH ({d} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.DestinationId)}}}) ");
                    sb.Append($"MATCH ({s})-");
                    sb.Append($"[{r}:`{set.Key}` {{" +
                        $"{nameof(OgmConnection.SourcePropertyName)}:{row}.{nameof(RelEntity.Properties)}.{nameof(OgmConnection.SourcePropertyName)}," +
                        $"{nameof(OgmConnection.DestinationPropertyName)}:{row}.{nameof(RelEntity.Properties)}.{nameof(OgmConnection.DestinationPropertyName)}," +
                        $"{nameof(OgmConnection.Order)}:{row}.{nameof(RelEntity.Properties)}.{nameof(OgmConnection.Order)}" +
                        $"}}]");
                    sb.Append($"->({d}) ");
                    sb.Append($"WHERE EXISTS({r}.{nameof(OgmConnection.Version)}) AND {r}.{nameof(OgmConnection.Version)}<>{row}.{nameof(RelEntity.Properties)}.{nameof(OgmConnection.Version)} ");
                    sb.Append($"DELETE {r}");

                    runner.Execute(sb.ToString(), batch);
                }                

                return results.OrderBy(p => p.Item1).Select(p => p.Item2 as IOgmConnection).ToList();
            }
        }

        public override IEnumerable<IOgmEntity> UpdateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.Item1?.EntityId != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"MATCH ({m} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(IOgmEntity.EntityId)}}})");
                sb.Append($"SET {m}+={row}.{nameof(NodeEntity.Properties)} ");
                sb.Append($"RETURN {m}");

                return runner.ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = entities.Select(p => new NodeEntity(p.Item1, excludePorperties: p.Item2).ToPropDictionary()).ToList() }).ToList();
            }
        }

        public override IEnumerable<IOgmConnection> UpdateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmConnection, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.Item1?.EntityId != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"MATCH ()-[{m} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(IOgmEntity.EntityId)}}}]->()");
                sb.Append($"SET {m}+={row}.{nameof(RelEntity.Properties)} ");
                sb.Append($"RETURN {m}");

                return runner.ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = entities.Select(p => new RelEntity(p.Item1, -1, -1, excludePorperties: p.Item2).ToPropDictionary()).ToList() }).ToList().Select(p=>p as IOgmConnection);
            }
        }
    }
}
