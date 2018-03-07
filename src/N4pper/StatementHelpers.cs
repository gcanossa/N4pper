using AsIKnow.Graph;
using N4pper.Decorators;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
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
        private static string ExpressionBody(Dictionary<string, object> value, Dictionary<string, object> replacements)
        {
            replacements = replacements ?? new Dictionary<string, object>();
            StringBuilder sb = new StringBuilder();

            sb.Append("{");

            foreach (KeyValuePair<string, object> kv in value)
            {
                sb.Append($"{kv.Key}:");
                sb.Append(replacements.ContainsKey(kv.Key) ? replacements[kv.Key] : JsonConvert.SerializeObject(kv.Value));
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);

            sb.Append("}");
            
            return sb.ToString();
        }
        
        public static string NodeExpression(Node node, string name = null, Dictionary<string, object> replacements = null)
        {
            node = node ?? throw new ArgumentNullException(nameof(node));
                        
            name = name ?? "";

            return NodeExpression(node.Labels, node.ToDictionary(p=>p.Key,p=>p.Value).SelectValueTypesProperties() , name, replacements);
        }
        public static string NodeExpression(IEnumerable<string> nodeLabels, Dictionary<string,object> nodeProps, string name = null, Dictionary<string, object> replacements = null)
        {
            nodeLabels = nodeLabels ?? throw new ArgumentNullException(nameof(nodeLabels));
            if (nodeLabels.Count() == 0) throw new ArgumentException($"{nameof(nodeLabels)} cannot be empty", nameof(nodeLabels));
            nodeProps = nodeProps ?? throw new ArgumentNullException(nameof(nodeProps));
            if (nodeProps.Count == 0) throw new ArgumentException($"{nameof(nodeProps)} cannot be empty", nameof(nodeProps));

            name = name ?? "";

            StringBuilder sb = new StringBuilder();
            sb.Append($"({name}");

            nodeLabels.ToList().ForEach(p => sb.Append($":{p}"));

            sb.Append(" ");
            sb.Append(ExpressionBody(nodeProps, replacements));

            sb.Append(")");

            return sb.ToString();
        }
        public static string RelationshipExpression(Relationship relationship, string name = null, Dictionary<string, object> replacements = null)
        {
            relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
                        
            name = name ?? "";

            StringBuilder sb = new StringBuilder();
            sb.Append($"[{name}");

            sb.Append($":{relationship.EntityType}");

            sb.Append(" ");
            sb.Append(ExpressionBody(relationship.ToDictionary(p => p.Key, p => p.Value), replacements));

            sb.Append("]");

            return sb.ToString();
        }
        public static string RelationshipExpression(string relType, Dictionary<string, object> relProps, string name = null, Dictionary<string, object> replacements = null)
        {
            if(string.IsNullOrEmpty(relType)) throw new ArgumentException($"{nameof(relType)} cannot be null or empty", nameof(relType));
            relProps = relProps ?? throw new ArgumentNullException(nameof(relProps));
            if (relProps.Count == 0) throw new ArgumentException($"{nameof(relProps)} cannot be empty", nameof(relProps));

            name = name ?? "";

            StringBuilder sb = new StringBuilder();
            sb.Append($"[{name}");

            sb.Append($":{relType}");

            sb.Append(" ");
            sb.Append(ExpressionBody(relProps, replacements));

            sb.Append("]");

            return sb.ToString();
        }

        public static string IdentityExpression(GraphEntity entity, string idName = "uuid", bool global = false)
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            if(!global)
                return $"MERGE (id:__UniqueId__{{name:'{entity.EntityType}'}}) ON CREATE SET id.count = 1 ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName}";
            else
                return $"MERGE (id:__GlobalUniqueId__) ON CREATE SET id.count = 1 ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName}";
        }
        public static string GlobalIdentityExpression(GraphEntity entity, string idName = "uuid")
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            return $"MERGE (id:GlobalUniqueId) ON CREATE SET id.count = 1 ON MATCH SET id.count = id.count + 1 WITH id.count AS {idName}";
        }
        public static string SetExpression(GraphEntity entity, string name, Dictionary<string, object> replacements = null)
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));
            if(string.IsNullOrEmpty(name)) throw new ArgumentException($"name cannot be null or empty", nameof(name));
            replacements = replacements ?? new Dictionary<string, object>();

            StringBuilder sb = new StringBuilder();
            
            foreach (KeyValuePair<string, object> kv in entity.ToDictionary(p => p.Key, p => p.Value).SelectValueTypesProperties())
            {
                sb.Append($"{name}.{kv.Key}=");
                sb.Append(replacements.ContainsKey(kv.Key) ? replacements[kv.Key] : JsonConvert.SerializeObject(kv.Value));
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
