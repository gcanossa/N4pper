using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    public class CypherQueryProvider : IQueryProvider
    {
        public IStatementRunner Runner { get; set; }
        public Statement Statement { get; set; }
        public Func<IRecord, Type, object> Mapper { get; set; }

        public CypherQueryProvider(IStatementRunner runner, Statement statement, Func<IRecord, Type, object> mapper)
        {
            Runner = runner ?? throw new ArgumentNullException(nameof(runner));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(QueryableNeo4jStatement<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        // Queryable's collection-returning standard query operators call this method. 
        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            return new QueryableNeo4jStatement<TResult>(Runner, Statement, Mapper, this, expression);
        }

        public object Execute(Expression expression)
        {
            return CypherQueryContext.Execute<object>(Runner, Statement, Mapper, expression);
        }

        // Queryable's "single value" standard query operators call this method.
        // It is also called from QueryableTerraServerData.GetEnumerator(). 
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)CypherQueryContext.Execute<TResult>(Runner, Statement, Mapper, expression);
        }
    }
}
