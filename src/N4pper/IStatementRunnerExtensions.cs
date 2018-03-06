using AsIKnow.Graph;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper
{
    public static class IStatementRunnerExtensions
    {
        private static GraphEntity Map(object obj, GraphManager manager)
        {
            if (obj is INode)
                return MapNode((INode)obj, manager);
            else if (obj is IRelationship)
                return MapRelationship((IRelationship)obj, manager);
            else
                throw new ArgumentException($"Unable to map type {obj.GetType().FullName}", nameof(obj));
        }
        private static Node MapNode(INode node, GraphManager manager)
        {
            Node result = manager.CreateNode();

            manager.Manager.GetTypesFromLabels(node.Labels).ToList().ForEach(p => result.AddLabel(p));

            result.SetProps(node.Properties);

            return result;
        }
        private static Relationship MapRelationship(IRelationship relationship, GraphManager manager)
        {
            Relationship result = manager.CreateRelationship();

            result.OfType(manager.Manager.GetTypesFromLabels(new[] { relationship.Type }).First());

            result.SetProps(relationship.Properties);

            return result;
        }

        public static IEnumerable<T> Query<T>(this IStatementRunner ext, string query, object param = null) where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            query = query ?? throw new ArgumentNullException(nameof(query));

            GraphManager mgr = (ext as IGraphManagedStatementRunner)?.Manager ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            IStatementResult result;
            if(param != null)
                result = ext.Run(query, param);
            else
                result = ext.Run(query);

            List<IRecord> records = result.ToList();

            if (result.Keys.Count < 1)
                throw new Exception("The query did not produced enough results");
            
            return records.Select(p => Map(p.Values[p.Keys[0]], mgr).FillObject<T>()).ToList();
        }
    }
}
