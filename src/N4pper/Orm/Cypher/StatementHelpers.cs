using N4pper.Decorators;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public static class StatementHelpers
    {
        public const string IdentityNodeLabel = "__UniqueId__";
        public const string GlobalIdentityNodeLabel = "__GlobalUniqueId__";

        private static Random RndGen = new Random();

        private static (int, long) GetSlot()
        {
            int tmp = RndGen.Next(Int32.MaxValue - 1);
            return (tmp, (Int64.MaxValue / Int32.MaxValue) * tmp );
        }
        
        public static string IdentityExpressionPrefix(string entityType, string idName = null)
        {
            idName = idName ?? "uuid";
            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException("cannot be null or empty", nameof(entityType));

            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{IdentityNodeLabel}{{name:'{entityType}', slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName} ";
        }
        public static string IdentityExpressionPrefix(IEnumerable<string> nodeLabels, string idName = null)
        {
            idName = idName ?? "uuid";
            nodeLabels = nodeLabels ?? throw new ArgumentNullException(nameof(nodeLabels));
            if (nodeLabels.Count() == 0) throw new ArgumentException("cannot be or empty", nameof(nodeLabels));

            string name = string.Join("_", nodeLabels.OrderBy(p=>p).ToArray());
            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{IdentityNodeLabel}{{name:'{name}', slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName} ";
        }

        public static string GlobalIdentityExpressionPrefix(string entityType, string idName = null)
        {
            idName = idName ?? "uuid";
            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException("cannot be null or empty", nameof(entityType));

            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{GlobalIdentityNodeLabel} {{slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName} ";
        }
        public static string GlobalIdentityExpressionPrefix(IEnumerable<string> nodeLabels, string idName = null)
        {
            idName = idName ?? "uuid";
            nodeLabels = nodeLabels ?? throw new ArgumentNullException(nameof(nodeLabels));
            if (nodeLabels.Count() == 0) throw new ArgumentException("cannot be or empty", nameof(nodeLabels));

            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{GlobalIdentityNodeLabel} {{slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName} ";
        }        
    }
}
