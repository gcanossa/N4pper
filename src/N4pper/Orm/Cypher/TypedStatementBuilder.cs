using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public abstract class TypedStatementBuilder<T> : StatementBuilder where T : class
    {
        public TypedStatementBuilder()
        {
        }
    }
}
