using N4pper.Queryable;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Queryable
{
    public class OrmCypherQueryProvider : CypherQueryProvider
    {
        public OrmCypherQueryProvider(IStatementRunner runner, Func<Statement> statement, Func<IRecord, Type, object> mapper) : base(runner, statement, mapper)
        {
        }
        protected override Type QueryableType => typeof(OrmQueryableNeo4jStatement<>);
    }
}
