using N4pper.Decorators;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace N4pper
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
        
        public static string IdentityExpression(string entityType, string idName = "uuid")
        {
            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException("cannot be null or empty", nameof(entityType));

            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{IdentityNodeLabel}{{name:'{entityType}', slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName}";
        }
        public static string IdentityExpression(IEnumerable<string> nodeLabels, string idName = "uuid")
        {
            nodeLabels = nodeLabels ?? throw new ArgumentNullException(nameof(nodeLabels));
            if (nodeLabels.Count() == 0) throw new ArgumentException("cannot be or empty", nameof(nodeLabels));

            string name = string.Join("_", nodeLabels.OrderBy(p=>p).ToArray());
            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{IdentityNodeLabel}{{name:'{name}', slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName}";
        }

        public static string GlobalIdentityExpression(string entityType, string idName = "uuid")
        {
            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException("cannot be null or empty", nameof(entityType));

            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{GlobalIdentityNodeLabel} {{slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName}";
        }
        public static string GlobalIdentityExpression(IEnumerable<string> nodeLabels, string idName = "uuid")
        {
            nodeLabels = nodeLabels ?? throw new ArgumentNullException(nameof(nodeLabels));
            if (nodeLabels.Count() == 0) throw new ArgumentException("cannot be or empty", nameof(nodeLabels));

            (int slot, long baseCount) = GetSlot();

            return $"MERGE (id:{GlobalIdentityNodeLabel} {{slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName}";
        }

        public static string NodeExpression(IEnumerable<string> nodeLabels, Dictionary<string,object> nodeProps, string name = null, Dictionary<string, object> symbolsOverride = null, string paramSuffix = null)
        {
            nodeLabels = nodeLabels ?? throw new ArgumentNullException(nameof(nodeLabels));
            if (nodeLabels.Count() == 0) throw new ArgumentException($"{nameof(nodeLabels)} cannot be empty", nameof(nodeLabels));
            nodeProps = nodeProps ?? throw new ArgumentNullException(nameof(nodeProps));

            name = name ?? "";

            StringBuilder sb = new StringBuilder();
            sb.Append($"({name}");

            nodeLabels.ToList().ForEach(p => sb.Append($":{p}"));

            sb.Append(" ");
            sb.Append(ExpressionBody(nodeProps, symbolsOverride, paramSuffix));

            sb.Append(")");

            return sb.ToString();
        }
        
        public static string RelationshipExpression(string relType, Dictionary<string, object> relProps, string name = null, Dictionary<string, object> symbolsOverride = null, string paramSuffix = null)
        {
            if(string.IsNullOrEmpty(relType)) throw new ArgumentException($"{nameof(relType)} cannot be null or empty", nameof(relType));
            relProps = relProps ?? throw new ArgumentNullException(nameof(relProps));

            name = name ?? "";

            StringBuilder sb = new StringBuilder();
            sb.Append($"[{name}");

            sb.Append($":{relType}");

            sb.Append(" ");
            sb.Append(ExpressionBody(relProps, symbolsOverride, paramSuffix));

            sb.Append("]");

            return sb.ToString();
        }

        private static string ExpressionBody(Dictionary<string, object> value, Dictionary<string, object> symbolsOverride, string paramSuffix)
        {
            if (value.Count == 0)
                return "";

            paramSuffix = paramSuffix ?? "";
            symbolsOverride = symbolsOverride ?? new Dictionary<string, object>();
            StringBuilder sb = new StringBuilder();

            sb.Append("{");

            foreach (KeyValuePair<string, object> kv in value)
            {
                sb.Append($"{kv.Key}:");
                sb.Append(symbolsOverride.ContainsKey(kv.Key) ? symbolsOverride[kv.Key] : $"${kv.Key}{paramSuffix}");
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);

            sb.Append("}");

            return sb.ToString();
        }
        
        public static string SetExpression(Dictionary<string, object> entity, string name, Dictionary<string, object> symbolsOverride = null, string paramSuffix = null)
        {
            if (entity == null || entity.Count==0) throw new ArgumentException("cannot be null or empty", nameof(entity));

            if (string.IsNullOrEmpty(name)) throw new ArgumentException($"name cannot be null or empty", nameof(name));

            paramSuffix = paramSuffix ?? "";
            symbolsOverride = symbolsOverride ?? new Dictionary<string, object>();

            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, object> kv in entity.SelectPrimitiveTypesProperties())
            {
                sb.Append($"{name}.{kv.Key}=");
                sb.Append(symbolsOverride.ContainsKey(kv.Key) ? symbolsOverride[kv.Key] : $"${kv.Key}{paramSuffix}");
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
