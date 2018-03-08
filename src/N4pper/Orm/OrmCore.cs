using AsIKnow.Graph;
using N4pper.Decorators;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm
{
    public static class OrmCore
    {
        private static Node GetNode<T>(T value, IGraphManagedStatementRunner mgr) where T : class
        {
            value = value ?? throw new ArgumentNullException(nameof(value));

            Node node = new Node(mgr.Manager.Manager.Manager, value);
            
            return node;
        }
        private static Relationship GetRelationship<T>(T value, IGraphManagedStatementRunner mgr) where T : class
        {
            value = value ?? throw new ArgumentNullException(nameof(value));

            Relationship rel = new Relationship(mgr.Manager.Manager.Manager, value);
            
            return rel;
        }
        
        public static T AddOrUpdateNode<T>(this IStatementRunner ext, T value) where T : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"type {typeof(T).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            value.ValidateObjectKeyValues();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node node = GetNode<T>(value, mgr);
            string n = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            Dictionary<string, object> symbolsOverride = null;
            if(value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                string uuid = OrmCoreHelpers.TempSymbol();
                sb.Append($"{StatementHelpers.GlobalIdentityExpression(node, uuid)} ");
                symbolsOverride = new Dictionary<string, object>() { { Constants.IdentityPropertyName, uuid } };
            }

            sb.Append($"MERGE {StatementHelpers.NodeExpression(node.Labels, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), n, symbolsOverride)} WITH {n} ");
            if(symbolsOverride!=null)
            {
                foreach (object item in symbolsOverride.Values)
                {
                    sb.Append($",{item} ");
                }
            }
            sb.Append($"SET {StatementHelpers.SetExpression(value.SelectPrimitiveTypesProperties(), n, symbolsOverride)} RETURN {n}");

            return ext.ExecuteQuery<T>(sb.ToString()).First();
        }
        public static int DeleteNode<T>(this IStatementRunner ext, T value) where T : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"type {typeof(T).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            value.ValidateObjectKeyValues();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node node = GetNode<T>(value, mgr);
            
            string n = OrmCoreHelpers.TempSymbol();

            return ext.Execute<T>($"MATCH {StatementHelpers.NodeExpression(node.Labels, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), n)} DELETE {n}").Counters.NodesDeleted;
        }
        
        public static T AddOrUpdateRel<T, S, D>(this IStatementRunner ext, T value, S source = null, D destination = null)
            where T : class, new()
            where S : class, new()
            where D : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"type {typeof(T).FullName} is unknown");
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(S)))
                throw new InvalidOperationException($"type {typeof(S).FullName} is unknown");
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(D)))
                throw new InvalidOperationException($"type {typeof(D).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            value.ValidateObjectKeyValues();
            if (source != null)
                source.ValidateObjectKeyValues();
            if (destination != null)
                destination.ValidateObjectKeyValues();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));
            
            Relationship rel = GetRelationship<T>(value, mgr);
            
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            Dictionary<string, object> symbolsOverride = null;
            if (value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                string uuid = OrmCoreHelpers.TempSymbol();
                sb.Append($"{StatementHelpers.GlobalIdentityExpression(rel, uuid)} ");
                symbolsOverride = new Dictionary<string, object>() { { Constants.IdentityPropertyName, uuid } };
            }

            sb.Append("MATCH ");

            string nS = OrmCoreHelpers.TempSymbol();
            if (source != null)
            {
                Node nodeS = GetNode<S>(source, mgr);
                sb.Append(StatementHelpers.NodeExpression(nodeS.Labels, source.SelectProperties(OrmCoreTypes.KnownTypes[typeof(S)]), nS));
            }
            else
            {
                sb.Append(StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<S>(), new Dictionary<string, object>(), nS));
            }
            sb.Append(" MATCH ");
            string nD = OrmCoreHelpers.TempSymbol();
            if (destination != null)
            {
                Node nodeD = GetNode<D>(destination, mgr);
                sb.Append(StatementHelpers.NodeExpression(nodeD.Labels, destination.SelectProperties(OrmCoreTypes.KnownTypes[typeof(D)]), nD));
            }
            else
            {
                sb.Append(StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<D>(), new Dictionary<string, object>(), nD));
            }
            if (source == null || destination == null)
            {
                sb.Append(" MATCH ");
                sb.Append($" ({nS})-");
                sb.Append(StatementHelpers.RelationshipExpression(rel.EntityType, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), r, symbolsOverride));
                sb.Append($"->({nD})");
            }
            else
            {
                sb.Append(" MERGE");
                sb.Append($" ({nS})-");

                sb.Append(StatementHelpers.RelationshipExpression(rel.EntityType, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), r, symbolsOverride));

                sb.Append($"->({nD})");
            }

            sb.Append($" WITH {r} ");

            if (source != null && destination != null)
            {
                if (symbolsOverride != null)
                {
                    foreach (object item in symbolsOverride.Values)
                    {
                        sb.Append($",{item} ");
                    }
                }
            }
            sb.Append($" SET {StatementHelpers.SetExpression(value.SelectPrimitiveTypesProperties(), r, symbolsOverride)} RETURN {r}");

            return ext.ExecuteQuery<T>(sb.ToString()).First();
        }
        
        public static int DeleteRel<T>(this IStatementRunner ext, T value)
            where T : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"type {typeof(T).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            value.ValidateObjectKeyValues();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));
            
            Relationship rel = GetRelationship<T>(value, mgr);
            
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH ()-{StatementHelpers.RelationshipExpression(rel.EntityType, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), r)}->()");

            sb.Append($" DELETE {r}");

            return ext.Execute<T>(sb.ToString()).Counters.RelationshipsDeleted;
        }
    }
}
