using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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

            Provider = new CypherQueryProvider(runner, ()=> { BuildStatement(); return Statement; }, mapper);
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

        protected void BuildStatement()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"MATCH {new Node(FirstSymbol, typeof(TData))} ");

            foreach (StringBuilder item in Builders)
            {
                sb.Append(item.ToString());
                sb.Append(" ");
            }

            sb.Append($"RETURN {FirstSymbol}");

            foreach (string item in ReturnStatements.SelectMany(p=>p))
            {
                sb.Append(",");
                sb.Append(item);
            }

            Statement = new Statement(sb.ToString());
        }

        protected List<StringBuilder> Builders { get; } = new List<StringBuilder>();
        protected List<List<string>> ReturnStatements { get; } = new List<List<string>>();
        protected Symbol FirstSymbol { get; } = new Symbol();

        protected IInclude<D> StartNewInclude<D>(IEnumerable<string> props, bool isEnumerable) where D : class
        {
            if (props.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(props));
            PropertyInfo pinfo = typeof(TData).GetProperty(props.First());

            Symbol to = new Symbol();
            Rel rel = new Rel(
                null,
                typeof(Entities.Connection),
                new { PropertyName = pinfo.Name }.ToPropDictionary());
            Node node = new Node(to, typeof(D));
            
            StringBuilder sb = new StringBuilder();
            Builders.Add(sb);
            List<string> returns = new List<string>();
            ReturnStatements.Add(returns);

            if (isEnumerable)
                returns.Add($"reverse(collect({to}))");
            else
                returns.Add(to);

            sb.Append($"OPTIONAL MATCH {new Node(FirstSymbol, typeof(TData))}-{rel}->{node}");

            return new IncludeQueryBuilder<D>(sb, returns);
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
    }
}
