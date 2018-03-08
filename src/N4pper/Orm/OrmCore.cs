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

            return ext.ExecuteQuery<T>(sb.ToString(), value).First();
        }
        public static IEnumerable<T> AddOrUpdateNodes<T>(this ITransaction ext, IEnumerable<T> value) where T : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"type {typeof(T).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            return value.Select(p=>ext.AddOrUpdateNode<T>(p));
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

            return ext.Execute<T>($"MATCH {StatementHelpers.NodeExpression(node.Labels, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), n)} DELETE {n}", value).Counters.NodesDeleted;
        }
        public static int DeleteNodes<T>(this ITransaction ext, IEnumerable<T> value) where T : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"type {typeof(T).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            return value.Sum(p => ext.DeleteNode<T>(p));
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

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            string nS = OrmCoreHelpers.TempSymbol();
            if (source != null)
            {
                Node nodeS = GetNode<S>(source, mgr);
                Dictionary<string, object> k = source.SelectProperties(OrmCoreTypes.KnownTypes[typeof(S)]);
                sb.Append(StatementHelpers.NodeExpression(nodeS.Labels, k, nS, new Dictionary<string, object>(), nameof(nS)));
                foreach (KeyValuePair<string, object> kv in k)
                {
                    parameters.Add($"{kv.Key}{nameof(nS)}", kv.Value);
                }
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
                Dictionary<string, object> k = destination.SelectProperties(OrmCoreTypes.KnownTypes[typeof(D)]);
                sb.Append(StatementHelpers.NodeExpression(nodeD.Labels, k, nD, new Dictionary<string, object>(), nameof(nD)));
                foreach (KeyValuePair<string, object> kv in k)
                {
                    parameters.Add($"{kv.Key}{nameof(nD)}", kv.Value);
                }
            }
            else
            {
                sb.Append(StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<D>(), new Dictionary<string, object>(), nD));
            }
            if (source == null || destination == null)
            {
                sb.Append(" MATCH ");
                sb.Append($" ({nS})-");
                sb.Append(StatementHelpers.RelationshipExpression(rel.EntityType, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), r, symbolsOverride, nameof(r)));
                sb.Append($"->({nD})");
            }
            else
            {
                sb.Append(" MERGE");
                sb.Append($" ({nS})-");

                sb.Append(StatementHelpers.RelationshipExpression(rel.EntityType, value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(T)]), r, symbolsOverride, nameof(r)));

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
            sb.Append($" SET {StatementHelpers.SetExpression(value.SelectPrimitiveTypesProperties(), r, symbolsOverride, nameof(r))} RETURN {r}");

            foreach (KeyValuePair<string, object> kv in value.SelectPrimitiveTypesProperties())
            {
                parameters.Add($"{kv.Key}{nameof(r)}", kv.Value);
            }

            return ext.ExecuteQuery<T>(sb.ToString(), parameters).First();
        }
        public static IEnumerable<T> AddOrUpdateRels<T, S, D>(this ITransaction ext, IEnumerable<Tuple<T, S, D>> value = null)
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

            return value.Select(p => ext.AddOrUpdateRel<T, S, D>(p.Item1, p.Item2, p.Item3));
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

            return ext.Execute<T>(sb.ToString(), value).Counters.RelationshipsDeleted;
        }
        public static int DeleteRels<T>(this ITransaction ext, IEnumerable<T> value)
            where T : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"type {typeof(T).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            return value.Sum(p=>ext.DeleteRel<T>(p));
        }

        public static IEnumerable<T> QueryForNode<T>(this IStatementRunner ext, object param = null) where T : class, new ()
        {
            return ext.QueryForNode<T,T>(param);
        }
        public static IEnumerable<Result> QueryForNode<T, Result>(this IStatementRunner ext, object param = null)
            where T : class
            where Result : class, T, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string n = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH {StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<T>(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), n)} RETURN {n} ");

            return ext.ExecuteQuery<Result>(sb.ToString(), param).Select(p => p);
        }

        public static IEnumerable<T> QueryForSourceNode<T, R>(this IStatementRunner ext, Func<T, R, T> map, object param = null, object relParam = null) 
            where T : class, new() 
            where R : class, new()
        {
            return ext.QueryForSourceNode<T,R,T,R>(map, param, relParam);
        }
        public static IEnumerable<TResult> QueryForSourceNode<T, R, TResult, RResult>(this IStatementRunner ext, Func<TResult, RResult, TResult> map, object param = null, object relParam = null)
            where T : class
            where R : class
            where TResult: class, T, new()
            where RResult : class, R, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? ((a, b) => a);

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string n = OrmCoreHelpers.TempSymbol();
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ");
            sb.Append(StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<T>(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), n));
            sb.Append("-");
            sb.Append(StatementHelpers.RelationshipExpression(mgr.Manager.Manager.Manager.GetLabel<R>(), relParam?.ToPropDictionary() ?? new Dictionary<string, object>(), r));
            sb.Append("->()");
            sb.Append($" RETURN {n},{r} ");

            return ext.ExecuteQuery<TResult, RResult>(sb.ToString(), map, param);
        }

        public static IEnumerable<T> QueryForDestinationNode<T, R>(this IStatementRunner ext, Func<T, R, T> map, object param = null, object relParam = null)
            where T : class, new()
            where R : class, new()
        {
            return ext.QueryForDestinationNode<T, R, T, R>(map, param, relParam);
        }
        public static IEnumerable<TResult> QueryForDestinationNode<T, R, TResult, RResult>(this IStatementRunner ext, Func<TResult, RResult, TResult> map, object param = null, object relParam = null)
            where T : class
            where R : class
            where TResult : class, T, new()
            where RResult : class, R, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? ((a, b) => a);

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string n = OrmCoreHelpers.TempSymbol();
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ");
            sb.Append("()-");
            sb.Append(StatementHelpers.RelationshipExpression(mgr.Manager.Manager.Manager.GetLabel<R>(), relParam?.ToPropDictionary() ?? new Dictionary<string, object>(), r));
            sb.Append("->");
            sb.Append(StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<T>(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), n));
            sb.Append($" RETURN {n},{r} ");

            return ext.ExecuteQuery<TResult, RResult>(sb.ToString(), map, param);
        }

        public static IEnumerable<T> QueryForRel<T>(this IStatementRunner ext, object param = null)
            where T : class, new()
        {
            return ext.QueryForRel<T, T>(param);
        }
        public static IEnumerable<TResult> QueryForRel<T, TResult>(this IStatementRunner ext, object param = null)
            where T : class
            where TResult : class, T, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ()-{StatementHelpers.RelationshipExpression(mgr.Manager.Manager.Manager.GetLabel<T>(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), r)}->() RETURN {r} ");

            return ext.ExecuteQuery<TResult>(sb.ToString(), param);
        }

        public static IEnumerable<T> QueryForRel<T, S, D>(this IStatementRunner ext, Func<T,S,D,T> map = null, object param = null, object sourceParam = null, object destinationParam = null)
            where T : class, new()
            where S : class, new()
            where D : class, new()
        {
            return ext.QueryForRel<T, S, D, T, S, D>(map, param, sourceParam, destinationParam);
        }
        public static IEnumerable<TResult> QueryForRel<T, S, D, TResult, SResult, DResult>(this IStatementRunner ext, Func<TResult, SResult, DResult, TResult> map = null, object param = null, object sourceParam = null, object destinationParam = null)
            where T : class
            where S : class
            where D : class
            where TResult : class, T, new()
            where SResult : class, S, new()
            where DResult : class, D, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? ((a, x, y) => a);

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string nS = OrmCoreHelpers.TempSymbol();
            string nD = OrmCoreHelpers.TempSymbol();
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ");
            sb.Append(StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<S>(), sourceParam?.ToPropDictionary() ?? new Dictionary<string, object>(), nS));
            sb.Append("-");
            sb.Append(StatementHelpers.RelationshipExpression(mgr.Manager.Manager.Manager.GetLabel<T>(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), r));
            sb.Append("->");
            sb.Append(StatementHelpers.NodeExpression(mgr.Manager.Manager.Manager.GetLabels<D>(), destinationParam?.ToPropDictionary() ?? new Dictionary<string, object>(), nD));
            sb.Append($" RETURN {r},{nS},{nD} ");

            return ext.ExecuteQuery<TResult, SResult, DResult>(sb.ToString(), map, param);
        }
    }
}
