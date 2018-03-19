using N4pper.QueryUtils;
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

            Symbol n = new Symbol();

            Node node = new Node(n,
                typeof(TNode),
                value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TNode)]));
            node.Parametrize();

            Set set = new Set(n,
                value.SelectPrimitiveTypesProperties()
                );
            set.Parametrize();

            StringBuilder sb = new StringBuilder();

            Symbol uuid = null;
            if (value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                uuid = new Symbol();
                UniqueIdStatement id = new UniqueIdStatement(uuid);

                sb.Append(id);
                sb.Append(" ");

                node.Props[Constants.IdentityPropertyName] = uuid;
                set.Props[Constants.IdentityPropertyName] = uuid;
            }

            sb.Append($"MERGE {node} WITH {n}");
            if (uuid != null)
                sb.Append($", {uuid}");
            sb.Append($" SET {set} RETURN {n}");
                        
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

            Symbol n = new Symbol();

            Node node = new Node(n,
                typeof(TNode),
                value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TNode)]));

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

            Symbol nS = new Symbol();
            Symbol r = new Symbol();
            Symbol nD = new Symbol();
            
            Node nodeS = new Node(nS,
                typeof(S),
                source?.SelectProperties(OrmCoreTypes.KnownTypes[typeof(S)]));
            Parameters nSp = nodeS.Parametrize(nameof(nS));
            parameters = parameters.MergeWith(nSp.Prepare(source?.SelectProperties(OrmCoreTypes.KnownTypes[typeof(S)])));

            Node nodeD = new Node(nD,
                typeof(D),
                destination?.SelectProperties(OrmCoreTypes.KnownTypes[typeof(D)]));
            Parameters nDp = nodeD.Parametrize(nameof(nD));
            parameters = parameters.MergeWith(nDp.Prepare(destination?.SelectProperties(OrmCoreTypes.KnownTypes[typeof(D)])));

            Set set = new Set(r,
                value.SelectPrimitiveTypesProperties());
            Parameters setR = set.Parametrize(nameof(r));
            parameters = parameters.MergeWith(setR.Prepare(value.SelectPrimitiveTypesProperties()));

            Rel rel = new Rel(r,
                typeof(TRel),
                value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TRel)]));
            rel.Parametrize(setR);

            StringBuilder sb = new StringBuilder();

            Symbol uuid = null;
            if (value.HasIdentityKey() && value.IsIdentityKeyNotSet())
            {
                uuid = new Symbol();
                UniqueIdStatement id = new UniqueIdStatement(uuid);

                sb.Append(id);
                sb.Append(" ");

                rel.Props[Constants.IdentityPropertyName] = uuid;
                set.Props[Constants.IdentityPropertyName] = uuid;

                rel.Props[Constants.IdentityPropertyName] = uuid;
                set.Props[Constants.IdentityPropertyName] = uuid;
            };

            string clause = (source==null || destination == null)? "MATCH" : "MERGE";

            sb.Append($"MATCH {nodeS} MATCH {nodeD} {clause} {new Node(nS)._(rel)._V(nD)} WITH {r}");
            if (uuid != null) sb.Append($", {uuid}");
            sb.Append($" SET {set} RETURN {r}");

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

            Symbol r = new Symbol();

            Rel rel = new Rel(r,
                typeof(TRel),
                value.SelectProperties(OrmCoreTypes.KnownTypes[typeof(TRel)]));
            rel.Parametrize();
                        
            return ext.Execute($"MATCH {new Node()._(rel)._()} DELETE {r}", value).Counters.RelationshipsDeleted;
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
    }
}
