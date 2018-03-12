using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public class SetExpressionBodyBuilder<T> : PropsExpressionBuilder<T> where T : class
    {
        public override string Template { get; } = "#props#";

        protected Symbol Scope { get; set; }

        public void ScopeProps(Symbol symbol)
        {
            Scope = symbol;
        }

        protected override string KVExpression(KeyValuePair<string, string> obj)
        {
            string key = (Scope != null ? $"{Scope}.{obj.Key}" : obj.Key);
            return $"{key}={obj.Value}";
        }
    }
}
