using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public class EntityExpressionBodyBuilder<T> : PropsExpressionBuilder<T> where T : class
    {
        protected override string KVExpression(KeyValuePair<string, string> obj)
        {
            return $"{obj.Key}:{obj.Value}";
        }
    }
}
