using N4pper.Decorators;
using N4pper.Orm.Cypher;
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
            
            Symbol n = Cypr.Symbol();
            
            NodeExpressionBuilder<TNode> node = Cypr.Node<TNode>(n);
            node.WithBody().SetValues(value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TNode)]));
            node.WithBody().Parametrize();

            SetExpressionBodyBuilder<TNode> body = Cypr.Set<TNode>(value.SelectPrimitiveTypesProperties(), n);
            body.Parametrize();

            StringBuilder sb = new StringBuilder();

            Symbol uuid = null;
            if (value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                uuid = Cypr.Symbol();
                sb.Append($"{Cypr.NodeId<TNode>(uuid)} ");

                node.WithBody().SetSymbol(Constants.IdentityPropertyName, uuid);
                body.SetSymbol(Constants.IdentityPropertyName, uuid);
            }

            body.ScopeProps(n);

            sb.Append($"MERGE {node} WITH {n}");
            if (uuid != null) sb.Append($", {uuid}");
            sb.Append($" SET {body} RETURN {n}");
                        
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

            Symbol n = Cypr.Symbol();

            NodeExpressionBuilder<TNode> node = Cypr.Node<TNode>(n);
            node.WithBody().SetValues(value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TNode)]));
            node.WithBody().Parametrize();

            return ext.Execute($"MATCH {node} DELETE {n}", value).Counters.NodesDeleted;
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

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            Symbol nS = Cypr.Symbol();
            Symbol r = Cypr.Symbol();
            Symbol nD = Cypr.Symbol();

            NodeExpressionBuilder<S> nodeS = Cypr.Node<S>(nS);
            nodeS.WithBody().SetValues(source?.SelectProperties(OrmCoreTypes.KnownTypes[typeof(S)]));
            parameters = parameters.MergeWith(nodeS.WithBody().Parametrize(nameof(nS)));

            NodeExpressionBuilder<D> nodeD = Cypr.Node<D>(nD);
            nodeD.WithBody().SetValues(destination?.SelectProperties(OrmCoreTypes.KnownTypes[typeof(D)]));
            parameters = parameters.MergeWith(nodeD.WithBody().Parametrize(nameof(nD)));

            RelationshipExpressionBuilder<TRel> rel = Cypr.Rel<TRel>(r);
            rel.WithBody().SetValues(value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TRel)]));

            SetExpressionBodyBuilder<TRel> body = Cypr.Set<TRel>(value.SelectPrimitiveTypesProperties(), r);
            parameters = parameters.MergeWith(body.Parametrize(nameof(r)));

            StringBuilder sb = new StringBuilder();

            Symbol uuid = null;
            if (value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                uuid = Cypr.Symbol();
                sb.Append($"{Cypr.RelId<TRel>(uuid)} ");

                rel.WithBody().SetSymbol(Constants.IdentityPropertyName, uuid);
                body.SetSymbol(Constants.IdentityPropertyName, uuid);
            };
            rel.WithBody().Parametrize(nameof(r));

            body.ScopeProps(r);

            string clause = (source==null || destination == null)? "MATCH" : "MERGE";

            sb.Append($"MATCH {nodeS} MATCH {nodeD} {clause} ({nS})-{rel}->({nD}) WITH {r}");
            if (uuid != null) sb.Append($", {uuid}");
            sb.Append($" SET {body} RETURN {r}");

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

            Symbol r = Cypr.Symbol();

            RelationshipExpressionBuilder<TRel> rel = Cypr.Rel<TRel>(r);
            rel.WithBody().SetValues(value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TRel)]));
            rel.WithBody().Parametrize();
            
            return ext.Execute($"MATCH ()-{rel}->() DELETE {r}", value).Counters.RelationshipsDeleted;
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

            Symbol n = Cypr.Symbol();

            StringBuilder sb = new StringBuilder();

            NodeExpressionBuilder<TNode> node = Cypr.Node<TNode>(n);
            node.WithBody().SetValues(param);
            node.WithBody().Parametrize();

            sb.Append($"MATCH {node} RETURN {n}");

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

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            Symbol n = Cypr.Symbol();
            Symbol r = Cypr.Symbol();

            NodeExpressionBuilder<TNode> node = Cypr.Node<TNode>(n);
            node.WithBody().SetValues(param);
            parameters = parameters.MergeWith(node.WithBody().Parametrize(nameof(n)));

            RelationshipExpressionBuilder<TRel> rel = Cypr.Rel<TRel>(r);
            rel.WithBody().SetValues(relParam);
            parameters = parameters.MergeWith(rel.WithBody().Parametrize(nameof(r)));

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH {node}-{rel}->() RETURN {n},{r}");

            return ext.ExecuteQuery<TNodeResult, TRelResult>(sb.ToString(), map, parameters);
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

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            Symbol n = Cypr.Symbol();
            Symbol r = Cypr.Symbol();

            NodeExpressionBuilder<TNode> node = Cypr.Node<TNode>(n);
            node.WithBody().SetValues(param);
            parameters = parameters.MergeWith(node.WithBody().Parametrize(nameof(n)));

            RelationshipExpressionBuilder<TRel> rel = Cypr.Rel<TRel>(r);
            rel.WithBody().SetValues(relParam);
            parameters = parameters.MergeWith(rel.WithBody().Parametrize(nameof(r)));

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ()-{rel}->{node} RETURN {n},{r}");

            return ext.ExecuteQuery<TNodeResult, TRelResult>(sb.ToString(), map, parameters);
        }
        
        public static IEnumerable<TResult> QueryForRel<TRel, TResult>(this IStatementRunner ext, object param = null)
            where TRel : class
            where TResult : class, TRel, new()
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            IGraphManagedStatementRunner mgr = (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(ext));

            Symbol r = Cypr.Symbol();

            RelationshipExpressionBuilder<TRel> rel = Cypr.Rel<TRel>(r);
            rel.WithBody().SetValues(param);
            rel.WithBody().Parametrize();

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH ()-{rel}->() RETURN {r} ");

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

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            Symbol nS = Cypr.Symbol();
            Symbol nD = Cypr.Symbol();
            Symbol r = Cypr.Symbol();

            NodeExpressionBuilder<S> nodeS = Cypr.Node<S>(nS);
            nodeS.WithBody().SetValues(sourceParam);
            parameters = parameters.MergeWith(nodeS.WithBody().Parametrize(nameof(nS)));

            NodeExpressionBuilder<D> nodeD = Cypr.Node<D>(nD);
            nodeD.WithBody().SetValues(destinationParam);
            parameters = parameters.MergeWith(nodeD.WithBody().Parametrize(nameof(nD)));

            RelationshipExpressionBuilder<TRel> rel = Cypr.Rel<TRel>(r);
            rel.WithBody().SetValues(param);
            parameters = parameters.MergeWith(rel.WithBody().Parametrize(nameof(r)));

            StringBuilder sb = new StringBuilder();

            sb.Append($"MATCH {nodeS}-{rel}->{nodeD} RETURN {r},{nS},{nD}");

            return ext.ExecuteQuery<TResult, SResult, DResult>(sb.ToString(), map, parameters);
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
