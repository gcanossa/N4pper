using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    public class QueryableNeo4jStatement<TData> : IOrderedQueryable<TData>
    {
        #region Constructors
        /// <summary> 
        /// This constructor is called by the client to create the data source. 
        /// </summary> 
        public QueryableNeo4jStatement()
        {
            Provider = new CypherQueryProvider();
            Expression = Expression.Constant(this);
        }

        /// <summary> 
        /// This constructor is called by Provider.CreateQuery(). 
        /// </summary> 
        /// <param name="expression"></param>
        public QueryableNeo4jStatement(CypherQueryProvider provider, Expression expression)
        {
            provider = provider ?? throw new ArgumentNullException(nameof(provider));
            expression = expression ?? throw new ArgumentNullException(nameof(expression));

            if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }

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
        public IEnumerator<TData> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<TData>>(Expression)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
        }
        #endregion
    }
}
