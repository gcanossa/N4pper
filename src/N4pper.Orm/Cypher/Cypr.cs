using OMnG;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public static class Cypr
    {
        public static Symbol Symbol(string value = null)
        {
            return new Symbol(value ?? $"_{Guid.NewGuid().ToString("N")}");
        }

        public static string NodeId<TNode>(Symbol symbol) where TNode : class
        {
            return StatementHelpers.GlobalIdentityExpressionPrefix(typeof(TNode).GetLabels(OrmCoreTypes.OMnGConfiguration), symbol);
        }
        public static string RelId<TRel>(Symbol symbol) where TRel : class
        {
            return StatementHelpers.GlobalIdentityExpressionPrefix(typeof(TRel).GetLabel(OrmCoreTypes.OMnGConfiguration), symbol);
        }

        public static NodeExpressionBuilder<TNode> Node<TNode>(Symbol symbol = null) where TNode : class
        {
            NodeExpressionBuilder<TNode> builder = new NodeExpressionBuilder<TNode>();

            if (symbol != null)
            {
                builder.SetSymbol(symbol);
            }

            foreach (string label in typeof(TNode).GetLabels(OrmCoreTypes.OMnGConfiguration))
            {
                builder.AddLabel(label);
            }

            return builder;
        }

        public static bool IsManagedType<T>() where T : class
        {
            return OrmCoreTypes.KnownTypes.ContainsKey(typeof(T));
        }

        public static string NodeLabels<TNode>() where TNode : class
        {
            string res = string.Join(":", typeof(TNode).GetLabels(OrmCoreTypes.OMnGConfiguration));
            if (res != "")
                res = ":" + res;
            return res;
        }
        public static string RelLabel<TRel>() where TRel : class
        {
            return ":" + typeof(TRel).GetLabel(OrmCoreTypes.OMnGConfiguration);
        }

        public static RelationshipExpressionBuilder<TRel> Rel<TRel>(Symbol symbol = null) where TRel : class
        {
            RelationshipExpressionBuilder<TRel> builder = new RelationshipExpressionBuilder<TRel>();

            if (symbol != null)
            {
                builder.SetSymbol(symbol);
            }

            builder.SetLabel(typeof(TRel).GetLabel(OrmCoreTypes.OMnGConfiguration));
            
            return builder;
        }

        public static SetExpressionBodyBuilder<T> Set<T>(T value, Symbol symbol) where T : class
        {
            return Set<T>(value?.ToPropDictionary(), symbol);
        }
        public static SetExpressionBodyBuilder<T> Set<T>(Dictionary<string, object> value, Symbol symbol) where T : class
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            if (value.Count == 0) throw new ArgumentException("cannot be null or empty", nameof(value));

            SetExpressionBodyBuilder<T> builder = new SetExpressionBodyBuilder<T>();

            foreach (KeyValuePair<string, object> kv in value)
            {
                builder.SetValue(kv.Key, kv.Value);
            }

            return builder;
        }
    }
}
