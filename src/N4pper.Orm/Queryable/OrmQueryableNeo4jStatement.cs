using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using N4pper.Orm.Design;
using N4pper.Queryable;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;

namespace N4pper.Orm.Queryable
{
    internal class OrmQueryableNeo4jStatement<TData> : IOrderedQueryable<TData>, IInclude<TData> where TData : class
    {
        public IStatementRunner Runner { get; set; }
        public Statement Statement { get; set; }
        public Func<IRecord, Type, object> Mapper { get; set; }

        #region Constructors
        /// <summary> 
        /// This constructor is called by the client to create the data source. 
        /// </summary> 
        public OrmQueryableNeo4jStatement(IStatementRunner runner, Func<IRecord, Type, object> mapper)
        {
            Runner = runner ?? throw new ArgumentNullException(nameof(runner));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            Provider = new CypherQueryProvider(runner, () => { BuildStatement(); return Statement; }, mapper);
            Expression = Expression.Constant(this);
        }

        /// <summary> 
        /// This constructor is called by Provider.CreateQuery(). 
        /// </summary> 
        /// <param name="expression"></param>
        public OrmQueryableNeo4jStatement(IStatementRunner runner, Func<Statement> statement, Func<IRecord, Type, object> mapper, CypherQueryProvider provider, Expression expression)
            : this(runner, mapper)
        {
            provider = provider ?? throw new ArgumentNullException(nameof(provider));
            expression = expression ?? throw new ArgumentNullException(nameof(expression));

            if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }

            Statement = statement();
            Provider = provider;
            Expression = expression;
        }
        #endregion

        #region Properties

        public IQueryProvider Provider { get; private set; }
        public Expression Expression { get; private set; }

        public Type ElementType
        {
            get { return typeof(TData); }
        }

        #endregion

        #region Enumerators
        public virtual IEnumerator<TData> GetEnumerator()
        {
            if (Statement == null)
                BuildStatement();
            return (Provider.Execute<IEnumerable<TData>>(Expression)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (Statement == null)
                BuildStatement();
            return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
        }
        #endregion
        
        private string BuildMatchStatement()
        {
            StringBuilder builder = new StringBuilder();
            Stack<Symbol> symbols = new Stack<Symbol>();
            symbols.Push(FirstSymbol);

            builder.Append($"MATCH {new Node(FirstSymbol, typeof(TData)).BuildForQuery()}");

            Paths.DepthFirstPaths((src,dst) =>
            {
                Type t =
                    dst.Item.IsReverse ?
                        dst.Item.IsEnumerable ? dst.Item.DestinationProperty.PropertyType.GetGenericArguments()[0] : dst.Item.DestinationProperty.PropertyType
                        :
                        dst.Item.IsEnumerable ? dst.Item.SourceProperty.PropertyType.GetGenericArguments()[0] : dst.Item.SourceProperty.PropertyType;

                Type r = dst.Item.Label == null ? typeof(Entities.Connection) : new string[] { dst.Item.Label }.GetTypesFromLabels().First();

                Rel rel = new Rel(dst.Item.RelSymbol, r, new Dictionary<string, object>()
                {
                    { nameof(dst.Item.SourceProperty), dst.Item.SourceProperty?.Name??"" },
                    { nameof(dst.Item.DestinationProperty), dst.Item.DestinationProperty?.Name??"" }
                });

                EdgeType ef = dst.Item.IsReverse ? EdgeType.From : EdgeType.Any;
                EdgeType es = dst.Item.IsReverse ? EdgeType.Any : EdgeType.To;
                builder.Append(" OPTIONAL MATCH ");
                builder.Append($"{new Node(src.Item.Symbol).BuildForQuery()}");
                builder.Append($"{ef.ToCypherString()}");
                builder.Append($"{rel}");
                builder.Append($"{es.ToCypherString()}");
                builder.Append($"{new Node(dst.Item.Symbol, t).BuildForQuery()}");
            });
            
            return builder.ToString();
        }
        
        private string BuildReturnStatement()
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append(" WITH ");

            Paths.DepthFirst(tree=>
            {
                if (tree.Item.IsEnumerable)
                    builder.Append($"{{rel:{tree.Item.RelSymbol},obj:{tree.Item.Symbol}}} AS {tree.Item.Symbol}");
                else
                    builder.Append($"{tree.Item.Symbol}");

                builder.Append($",");
            });
            builder.Remove(builder.Length - 1, 1);

            Dictionary<ITree<IncludePathComponent>, IncludePathComponent> replacements = new Dictionary<ITree<IncludePathComponent>, IncludePathComponent>();
            List<ITree<IncludePathComponent>> symbols = new List<ITree<IncludePathComponent>>();
            Paths.DepthFirst(tree => symbols.Add(tree));
            Paths.BreathFirstLevels(trees=>
            {
                List<ITree<IncludePathComponent>> branches = trees.SelectMany(p => p.Branches).ToList();

                if (trees.First() != Paths)
                    symbols.RemoveAll(p => trees.Any(q => q == p));

                StringBuilder sb = new StringBuilder();

                sb.Append($" WITH ");

                if (branches.Count > 0)
                {
                    sb.Append(string.Join(",", symbols.Select(p => p.Item.Symbol)));

                    foreach (IncludePathTree branch in trees)
                    {
                        Symbol s = new Symbol();
                        sb.Append($",{{this:{branch.Item.Symbol}");

                        foreach (IncludePathTree item in branch.Branches)
                        {
                            IncludePathComponent path = replacements.ContainsKey(item) ? replacements[item] : item.Item;

                            sb.Append(",");
                            sb.Append($"{path.Property.Name}:");
                            if (path.IsEnumerable)
                            {
                                sb.Append("collect(distinct ");
                            }
                            sb.Append(path.Symbol);
                            if (path.IsEnumerable)
                            {
                                sb.Append(")");
                            }
                        }
                        sb.Append($"}} AS {s}");
                        replacements.Add(branch, new IncludePathComponent() { Property = branch.Item.Property, IsEnumerable = branch.Item.IsEnumerable, Symbol = s, RelSymbol = branch.Item.RelSymbol });
                    }
                }
                else
                {
                    sb.Append(string.Join(",", symbols.Select(p => p.Item.Symbol)));
                    foreach (IncludePathTree branch in trees)
                    {
                        Symbol s = new Symbol();
                        sb.Append($",{{this:{branch.Item.Symbol}}} AS {s}");
                        replacements.Add(branch, new IncludePathComponent() { Property = branch.Item.Property, IsEnumerable = branch.Item.IsEnumerable, Symbol = s, RelSymbol = branch.Item.RelSymbol });
                    }
                }

                builder.Append(sb.ToString());
            },false);
            
            builder.Append($" RETURN {FirstSymbol}");
            if (replacements.Keys.Count > 0)
                builder.Append($",{replacements[Paths].Symbol}");

            return builder.ToString();
        }
        protected void BuildStatement()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(BuildMatchStatement());
            sb.Append(BuildReturnStatement());
            
            Statement = new Statement(sb.ToString());
        }

        protected ITree<IncludePathComponent> Paths { get; } = new IncludePathTree() { Item = new IncludePathComponent() { IsEnumerable=false, Symbol=new Symbol() } };
        protected Symbol FirstSymbol => Paths.Item.Symbol;

        protected IInclude<D> StartNewInclude<D>(IEnumerable<string> props, bool isEnumerable, string label = null) where D : class
        {
            if (props.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(props));
            PropertyInfo pinfo = typeof(TData).GetProperty(props.First());

            PropertyInfo sPinfo =
                OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(pinfo) ?
                pinfo :
                OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(pinfo) ?
                OrmCoreTypes.KnownTypeDestinationRelations[pinfo] ?? null :
                pinfo;
            PropertyInfo dPinfo =
                OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(sPinfo) ?
                OrmCoreTypes.KnownTypeSourceRelations[sPinfo] ?? null :
                null;

            bool isReverse = OrmCoreTypes.KnownTypeDestinationRelations.ContainsKey(pinfo) && !OrmCoreTypes.KnownTypeSourceRelations.ContainsKey(pinfo);

            Symbol to = new Symbol();
            Symbol toRel = new Symbol();
            ITree<IncludePathComponent> newTree = new IncludePathTree()
            {
                Item = new IncludePathComponent()
                {
                    IsReverse = isReverse,
                    SourceProperty = sPinfo,
                    DestinationProperty = dPinfo,
                    IsEnumerable = isEnumerable,
                    Symbol = to,
                    RelSymbol = toRel,
                    Label = label
                } 
            };

            newTree = Paths.Add(newTree);

            return new IncludeQueryBuilder<D>(newTree);
        }

        public IInclude<D> Include<D>(Expression<Func<TData, D>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));
            return StartNewInclude<D>(expr.ToPropertyNameCollection(), false);
        }

        public IInclude<D> Include<D>(Expression<Func<TData, IEnumerable<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));
            return StartNewInclude<D>(expr.ToPropertyNameCollection(), true);
        }

        public IInclude<D> Include<D>(Expression<Func<TData, IList<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));
            return StartNewInclude<D>(expr.ToPropertyNameCollection(), true);
        }

        public IInclude<D> Include<D>(Expression<Func<TData, List<D>>> expr) where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));
            return StartNewInclude<D>(expr.ToPropertyNameCollection(), true);
        }

        public IInclude<D> Include<C, D>(Expression<Func<TData, C>> expr)
            where C : Entities.ExplicitConnection<TData, D>
            where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return StartNewInclude<D>(expr.ToPropertyNameCollection(), false, OMnG.TypeExtensions.GetLabel(typeof(C)));
        }

        public IInclude<D> Include<C, D>(Expression<Func<TData, IEnumerable<C>>> expr)
            where C : Entities.ExplicitConnection<TData, D>
            where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return StartNewInclude<D>(expr.ToPropertyNameCollection(), true, OMnG.TypeExtensions.GetLabel(typeof(C)));
        }

        public IInclude<D> Include<C, D>(Expression<Func<TData, IList<C>>> expr)
            where C : Entities.ExplicitConnection<TData, D>
            where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return StartNewInclude<D>(expr.ToPropertyNameCollection(), true, OMnG.TypeExtensions.GetLabel(typeof(C)));
        }

        public IInclude<D> Include<C, D>(Expression<Func<TData, List<C>>> expr)
            where C : Entities.ExplicitConnection<TData, D>
            where D : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            return StartNewInclude<D>(expr.ToPropertyNameCollection(), true, OMnG.TypeExtensions.GetLabel(typeof(C)));
        }
    }
}
