using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    public class CypherQueryProvider : IQueryProvider
    {
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
            return new QueryableNeo4jStatement<TResult>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return CypherQueryContext.Execute(expression, false);
        }

        // Queryable's "single value" standard query operators call this method.
        // It is also called from QueryableTerraServerData.GetEnumerator(). 
        public TResult Execute<TResult>(Expression expression)
        {
            bool IsEnumerable = (typeof(TResult).Name == "IEnumerable`1");

            return (TResult)CypherQueryContext.Execute(expression, IsEnumerable);
        }
    }
}
