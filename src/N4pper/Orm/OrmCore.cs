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
        #region C_UD ops

        public static TResult AddOrUpdateNode<TNode, TResult>(this IStatementRunner ext, TNode value)
            where TNode : class
            where TResult : class, TNode, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TNode)))
                throw new InvalidOperationException($"type {typeof(TNode).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            value.ValidateObjectKeyValues();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));
            
            string n = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            Dictionary<string, object> symbolsOverride = null;
            if(value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                string uuid = OrmCoreHelpers.TempSymbol();
                sb.Append($"{StatementHelpers.GlobalIdentityExpression(typeof(TNode).GetLabels(), uuid)} ");
                symbolsOverride = new Dictionary<string, object>() { { Constants.IdentityPropertyName, uuid } };
            }

            sb.Append($"MERGE {StatementHelpers.NodeExpression(typeof(TNode).GetLabels(), value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TNode)]), n, symbolsOverride)} WITH {n} ");
            if(symbolsOverride!=null)
            {
                foreach (object item in symbolsOverride.Values)
                {
                    sb.Append($",{item} ");
                }
            }
            sb.Append($"SET {StatementHelpers.SetExpression(value.SelectPrimitiveTypesProperties(), n, symbolsOverride)} RETURN {n}");

            return ext.ExecuteQuery<TResult>(sb.ToString(), value).First();
        }
        public static IEnumerable<TResult> AddOrUpdateNodes<TNode, TResult>(this ITransaction ext, IEnumerable<TNode> value)
            where TNode : class
            where TResult : class, TNode, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TNode)))
                throw new InvalidOperationException($"type {typeof(TNode).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            return value.Select(p=>ext.AddOrUpdateNode<TNode,TResult>(p));
        }
        public static int DeleteNode<TNode>(this IStatementRunner ext, TNode value) where TNode : class
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TNode)))
                throw new InvalidOperationException($"type {typeof(TNode).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            value.ValidateObjectKeyValues();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));
                        
            string n = OrmCoreHelpers.TempSymbol();

            return ext.Execute($"MATCH {StatementHelpers.NodeExpression(typeof(TNode).GetLabels(), value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TNode)]), n)} DELETE {n}", value).Counters.NodesDeleted;
        }
        public static int DeleteNodes<TNode>(this ITransaction ext, IEnumerable<TNode> value) where TNode : class
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TNode)))
                throw new InvalidOperationException($"type {typeof(TNode).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            return value.Sum(p => ext.DeleteNode<TNode>(p));
        }

        public static TResult AddOrUpdateRel<TRel, TResult , S, D>(this IStatementRunner ext, TRel value, S source = null, D destination = null)
            where TRel : class
            where TResult : class, TRel, new()
            where S : class, new()
            where D : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TRel)))
                throw new InvalidOperationException($"type {typeof(TRel).FullName} is unknown");
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
                        
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            Dictionary<string, object> symbolsOverride = null;
            if (value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                string uuid = OrmCoreHelpers.TempSymbol();
                sb.Append($"{StatementHelpers.GlobalIdentityExpression(typeof(TRel).GetLabel(), uuid)} ");
                symbolsOverride = new Dictionary<string, object>() { { Constants.IdentityPropertyName, uuid } };
            }

            sb.Append("MATCH ");

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            string nS = OrmCoreHelpers.TempSymbol();
            if (source != null)
            {
                Dictionary<string, object> k = source.SelectProperties(OrmCoreTypes.KnownTypes[typeof(S)]);
                sb.Append(StatementHelpers.NodeExpression(typeof(S).GetLabels(), k, nS, new Dictionary<string, object>(), nameof(nS)));
                foreach (KeyValuePair<string, object> kv in k)
                {
                    parameters.Add($"{kv.Key}{nameof(nS)}", kv.Value);
                }
            }
            else
            {
                sb.Append(StatementHelpers.NodeExpression(typeof(S).GetLabels(), new Dictionary<string, object>(), nS));
            }
            sb.Append(" MATCH ");
            string nD = OrmCoreHelpers.TempSymbol();
            if (destination != null)
            {
                Dictionary<string, object> k = destination.SelectProperties(OrmCoreTypes.KnownTypes[typeof(D)]);
                sb.Append(StatementHelpers.NodeExpression(typeof(D).GetLabels(), k, nD, new Dictionary<string, object>(), nameof(nD)));
                foreach (KeyValuePair<string, object> kv in k)
                {
                    parameters.Add($"{kv.Key}{nameof(nD)}", kv.Value);
                }
            }
            else
            {
                sb.Append(StatementHelpers.NodeExpression(typeof(D).GetLabels(), new Dictionary<string, object>(), nD));
            }
            if (source == null || destination == null)
            {
                sb.Append(" MATCH ");
                sb.Append($" ({nS})-");
                sb.Append(StatementHelpers.RelationshipExpression(typeof(TRel).GetLabel(), value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TRel)]), r, symbolsOverride, nameof(r)));
                sb.Append($"->({nD})");
            }
            else
            {
                sb.Append(" MERGE");
                sb.Append($" ({nS})-");

                sb.Append(StatementHelpers.RelationshipExpression(typeof(TRel).GetLabel(), value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TRel)]), r, symbolsOverride, nameof(r)));

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

            return ext.ExecuteQuery<TResult>(sb.ToString(), parameters).First();
        }
        public static IEnumerable<TResult> AddOrUpdateRels<TRel, TResult, S, D>(this ITransaction ext, IEnumerable<Tuple<TRel, S, D>> value = null)
            where TRel : class
            where TResult : class, TRel, new()
            where S : class, new()
            where D : class, new()
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TRel)))
                throw new InvalidOperationException($"type {typeof(TRel).FullName} is unknown");
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(S)))
                throw new InvalidOperationException($"type {typeof(S).FullName} is unknown");
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(D)))
                throw new InvalidOperationException($"type {typeof(D).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            return value.Select(p => ext.AddOrUpdateRel<TRel, TResult, S, D>(p.Item1, p.Item2, p.Item3));
        }
        public static int DeleteRel<TRel>(this IStatementRunner ext, TRel value)
            where TRel : class
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TRel)))
                throw new InvalidOperationException($"type {typeof(TRel).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            value.ValidateObjectKeyValues();

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));
                        
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH ()-{StatementHelpers.RelationshipExpression(typeof(TRel).GetLabel(), value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TRel)]), r)}->()");

            sb.Append($" DELETE {r}");

            return ext.Execute(sb.ToString(), value).Counters.RelationshipsDeleted;
        }
        public static int DeleteRels<TRel>(this ITransaction ext, IEnumerable<TRel> value)
            where TRel : class
        {
            if (!OrmCoreTypes.KnownTypes.ContainsKey(typeof(TRel)))
                throw new InvalidOperationException($"type {typeof(TRel).FullName} is unknown");
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            value = value ?? throw new ArgumentNullException(nameof(value));

            return value.Sum(p=>ext.DeleteRel<TRel>(p));
        }

        #endregion

        #region C_UD ops overrides
        
        public static TNode AddOrUpdateNode<TNode>(this IStatementRunner ext, TNode value)
            where TNode : class,new()
        {
            return ext.AddOrUpdateNode<TNode, TNode>(value);
        }
        public static IEnumerable<TNode> AddOrUpdateNodes<TNode>(this ITransaction ext, IEnumerable<TNode> value)
            where TNode : class, new()
        {
            return ext.AddOrUpdateNodes<TNode, TNode>(value);
        }

        public static TRel AddOrUpdateRel<TRel, S, D>(this IStatementRunner ext, TRel value, S source = null, D destination = null)
            where TRel : class, new()
            where S : class, new()
            where D : class, new()
        {
            return ext.AddOrUpdateRel<TRel, TRel, S, D>(value, source, destination);
        }
        public static IEnumerable<TRel> AddOrUpdateRels<TRel, S, D>(this ITransaction ext, IEnumerable<Tuple<TRel, S, D>> value = null)
            where TRel : class, new()
            where S : class, new()
            where D : class, new()
        {
            return ext.AddOrUpdateRels<TRel, TRel, S, D>(value);
        }

        #endregion

        #region _R__ ops
        
        public static IEnumerable<TResult> QueryForNode<TNode, TResult>(this IStatementRunner ext, object param = null)
            where TNode : class
            where TResult : class, TNode, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string n = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH {StatementHelpers.NodeExpression(typeof(TNode).GetLabels(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), n)} RETURN {n} ");

            return ext.ExecuteQuery<TResult>(sb.ToString(), param).Select(p => p);
        }
        
        public static IEnumerable<TNodeResult> QueryForSourceNode<TNode, TRel, TNodeResult, TRelResult>(this IStatementRunner ext, Func<TNodeResult, TRelResult, TNodeResult> map, object param = null, object relParam = null)
            where TNode : class
            where TRel : class
            where TNodeResult : class, TNode, new()
            where TRelResult : class, TRel, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? ((a, b) => a);

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string n = OrmCoreHelpers.TempSymbol();
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ");
            sb.Append(StatementHelpers.NodeExpression(typeof(TNode).GetLabels(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), n));
            sb.Append("-");
            sb.Append(StatementHelpers.RelationshipExpression(typeof(TRel).GetLabel(), relParam?.ToPropDictionary() ?? new Dictionary<string, object>(), r));
            sb.Append("->()");
            sb.Append($" RETURN {n},{r} ");

            return ext.ExecuteQuery<TNodeResult, TRelResult>(sb.ToString(), map, param);
        }
        
        public static IEnumerable<TNodeResult> QueryForDestinationNode<TNode, TRel, TNodeResult, TRelResult>(this IStatementRunner ext, Func<TNodeResult, TRelResult, TNodeResult> map, object param = null, object relParam = null)
            where TNode : class
            where TRel : class
            where TNodeResult : class, TNode, new()
            where TRelResult : class, TRel, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            map = map ?? ((a, b) => a);

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string n = OrmCoreHelpers.TempSymbol();
            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ");
            sb.Append("()-");
            sb.Append(StatementHelpers.RelationshipExpression(typeof(TRel).GetLabel(), relParam?.ToPropDictionary() ?? new Dictionary<string, object>(), r));
            sb.Append("->");
            sb.Append(StatementHelpers.NodeExpression(typeof(TNode).GetLabels(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), n));
            sb.Append($" RETURN {n},{r} ");

            return ext.ExecuteQuery<TNodeResult, TRelResult>(sb.ToString(), map, param);
        }
        
        public static IEnumerable<TResult> QueryForRel<TRel, TResult>(this IStatementRunner ext, object param = null)
            where TRel : class
            where TResult : class, TRel, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            string r = OrmCoreHelpers.TempSymbol();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ()-{StatementHelpers.RelationshipExpression(typeof(TRel).GetLabel(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), r)}->() RETURN {r} ");

            return ext.ExecuteQuery<TResult>(sb.ToString(), param);
        }
        
        public static IEnumerable<TResult> QueryForRel<TRel, S, D, TResult, SResult, DResult>(this IStatementRunner ext, Func<TResult, SResult, DResult, TResult> map = null, object param = null, object sourceParam = null, object destinationParam = null)
            where TRel : class
            where S : class
            where D : class
            where TResult : class, TRel, new()
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
            sb.Append(StatementHelpers.NodeExpression(typeof(S).GetLabels(), sourceParam?.ToPropDictionary() ?? new Dictionary<string, object>(), nS));
            sb.Append("-");
            sb.Append(StatementHelpers.RelationshipExpression(typeof(TRel).GetLabel(), param?.ToPropDictionary() ?? new Dictionary<string, object>(), r));
            sb.Append("->");
            sb.Append(StatementHelpers.NodeExpression(typeof(D).GetLabels(), destinationParam?.ToPropDictionary() ?? new Dictionary<string, object>(), nD));
            sb.Append($" RETURN {r},{nS},{nD} ");

            return ext.ExecuteQuery<TResult, SResult, DResult>(sb.ToString(), map, param);
        }

        #endregion

        #region _R__ ops overrides

        public static IEnumerable<T> QueryForNode<T>(this IStatementRunner ext, object param = null) where T : class, new ()
        {
            return ext.QueryForNode<T,T>(param);
        }

        public static IEnumerable<TNode> QueryForSourceNode<TNode, TRel>(this IStatementRunner ext, Func<TNode, TRel, TNode> map, object param = null, object relParam = null) 
            where TNode : class, new() 
            where TRel : class, new()
        {
            return ext.QueryForSourceNode<TNode,TRel,TNode,TRel>(map, param, relParam);
        }

        public static IEnumerable<TNode> QueryForDestinationNode<TNode, TRel>(this IStatementRunner ext, Func<TNode, TRel, TNode> map, object param = null, object relParam = null)
            where TNode : class, new()
            where TRel : class, new()
        {
            return ext.QueryForDestinationNode<TNode, TRel, TNode, TRel>(map, param, relParam);
        }
        
        public static IEnumerable<TRel> QueryForRel<TRel>(this IStatementRunner ext, object param = null)
            where TRel : class, new()
        {
            return ext.QueryForRel<TRel, TRel>(param);
        }
        
        public static IEnumerable<TRel> QueryForRel<TRel, S, D>(this IStatementRunner ext, Func<TRel,S,D,TRel> map = null, object param = null, object sourceParam = null, object destinationParam = null)
            where TRel : class, new()
            where S : class, new()
            where D : class, new()
        {
            return ext.QueryForRel<TRel, S, D, TRel, S, D>(map, param, sourceParam, destinationParam);
        }
        
        #endregion
    }
}
