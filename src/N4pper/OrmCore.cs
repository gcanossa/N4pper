using AsIKnow.Graph;
using N4pper.Decorators;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper
{
    public static class OrmCore
    {        
        private static string TempSymbol(string basename = null)
        {
            basename = basename ?? "_";
            return $"{basename}{Guid.NewGuid().ToString("N")}";
        }
        private static object GetDefault(Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private static Node GetNode<T>(T value, IGraphManagedStatementRunner mgr, bool mustHaveIdentity = false) where T : class
        {
            value = value ?? throw new ArgumentNullException(nameof(value));

            Node node = new Node(mgr.Manager.Manager, value);

            if (mustHaveIdentity && !node.PropertiesKeys.Contains(mgr.Options.DefaultIdPropertyName))
                throw new ArgumentException($"Object missing identity property '{mgr.Options.DefaultIdPropertyName}'", nameof(value));
            if (mustHaveIdentity && node.PropertiesKeys.Contains(mgr.Options.DefaultIdPropertyName) && node[mgr.Options.DefaultIdPropertyName] == GetDefault(mgr.Options.DefaultIdPropertyType))
                throw new ArgumentException($"Object identity property set to its default '{mgr.Options.DefaultIdPropertyName}'", nameof(value));

            return node;
        }
        private static Relationship GetRelationship<T>(T value, IGraphManagedStatementRunner mgr, bool mustHaveIdentity = false) where T : class
        {
            value = value ?? throw new ArgumentNullException(nameof(value));

            Relationship rel = new Relationship(mgr.Manager.Manager, value);

            if (mustHaveIdentity && !rel.PropertiesKeys.Contains(mgr.Options.DefaultIdPropertyName))
                throw new ArgumentException($"Object missing identity property '{mgr.Options.DefaultIdPropertyName}'", nameof(value));
            if (mustHaveIdentity && rel.PropertiesKeys.Contains(mgr.Options.DefaultIdPropertyName) && rel[mgr.Options.DefaultIdPropertyName] == GetDefault(mgr.Options.DefaultIdPropertyType))
                throw new ArgumentException($"Object identity property set to its default '{mgr.Options.DefaultIdPropertyName}'", nameof(value));

            return rel;
        }

        public static T NewNode<T>(this IStatementRunner ext, T value) where T: class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node node = GetNode<T>(value, mgr);

            string n = TempSymbol();

            return ext.ExecuteQuery<T>($"CREATE {StatementHelpers.NodeExpression(node, n)} RETURN {n}").First();
        }
        public static T NewNodeUnique<T>(this IStatementRunner ext, T value, bool global = true) where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node node = GetNode<T>(value, mgr);

            string n = TempSymbol();
            string uuid = TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"{StatementHelpers.IdentityExpression(node, uuid, global)} CREATE ");
            sb.Append(StatementHelpers.NodeExpression(node, n, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, uuid } }));
            sb.Append($" RETURN {n}");

            return ext.ExecuteQuery<T>(sb.ToString()).First();
        }
        public static T AddOrUpdateNode<T>(this IStatementRunner ext, T value) where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node node = GetNode<T>(value, mgr, true);
            string n = TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH {StatementHelpers.NodeExpression(node.Labels, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, node[mgr.Options.DefaultIdPropertyName] } }, n)} SET {StatementHelpers.SetExpression(node, n)} RETURN {n}");

            return ext.ExecuteQuery<T>(sb.ToString()).First();
        }
        public static int DeleteNode<T>(this IStatementRunner ext, T value) where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node node = GetNode<T>(value, mgr, true);
            
            string n = TempSymbol();

            return ext.Execute<T>($"MATCH {StatementHelpers.NodeExpression(node.Labels, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, node[mgr.Options.DefaultIdPropertyName] } }, n)} DELETE {n}").Counters.NodesDeleted;
        }

        public static T NewRel<T, S, D>(this IStatementRunner ext, T value, S source, D destination) 
            where T : class, new()
            where S : class
            where D : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node nodeS = GetNode<S>(source, mgr, true);
            Node nodeD = GetNode<D>(destination, mgr, true);

            Relationship rel = GetRelationship<T>(value, mgr);

            string nS = TempSymbol();
            string nD = TempSymbol();
            string r = TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH {StatementHelpers.NodeExpression(nodeS.Labels, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, nodeS[mgr.Options.DefaultIdPropertyName] } }, nS)}");
            sb.Append($"MATCH {StatementHelpers.NodeExpression(nodeD.Labels, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, nodeD[mgr.Options.DefaultIdPropertyName] } }, nD)}");

            sb.Append($"CREATE ({nS})-{StatementHelpers.RelationshipExpression(rel, r)}->({nD}) RETURN {r}");

            return ext.ExecuteQuery<T>(sb.ToString()).First();
        }
        public static T NewRelUnique<T, S, D>(this IStatementRunner ext, T value, S source, D destination, bool global = true)
            where T : class, new()
            where S : class
            where D : class
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Node nodeS = GetNode<S>(source, mgr, true);
            Node nodeD = GetNode<D>(destination, mgr, true);

            Relationship rel = GetRelationship<T>(value, mgr);

            string nS = TempSymbol();
            string nD = TempSymbol();
            string r = TempSymbol();

            string uuid = TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH {StatementHelpers.NodeExpression(nodeS.Labels, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, nodeS[mgr.Options.DefaultIdPropertyName] } }, nS)}");
            sb.Append($"MATCH {StatementHelpers.NodeExpression(nodeD.Labels, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, nodeD[mgr.Options.DefaultIdPropertyName] } }, nD)}");
            sb.Append($" WITH {nS}, {nD} ");
            sb.Append($"{StatementHelpers.IdentityExpression(rel, uuid, global)}, {nS}, {nD} CREATE ");
            sb.Append($"({nS})-{StatementHelpers.RelationshipExpression(rel, r, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, uuid } })}->({nD})");
            sb.Append($" RETURN {r}");

            return ext.ExecuteQuery<T>(sb.ToString()).First();
        }
        public static T AddOrUpdateRel<T>(this IStatementRunner ext, T value)
            where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));
            
            Relationship rel = GetRelationship<T>(value, mgr, true);
            
            string r = TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH ()-{StatementHelpers.RelationshipExpression(rel.EntityType, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, rel[mgr.Options.DefaultIdPropertyName] } }, r)}->()");

            sb.Append($"SET {StatementHelpers.SetExpression(rel, r)} RETURN {r}");

            return ext.ExecuteQuery<T>(sb.ToString()).First();
        }
        public static int DeleteRel<T>(this IStatementRunner ext, T value)
            where T : class, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));
            
            Relationship rel = GetRelationship<T>(value, mgr, true);
            
            string r = TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH ()-{StatementHelpers.RelationshipExpression(rel.EntityType, new Dictionary<string, object>() { { mgr.Options.DefaultIdPropertyName, rel[mgr.Options.DefaultIdPropertyName] } }, r)}->()");

            sb.Append($" DELETE {r}");

            return ext.Execute<T>(sb.ToString()).Counters.RelationshipsDeleted;
        }
    }
}
