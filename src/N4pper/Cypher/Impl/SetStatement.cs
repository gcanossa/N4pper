using Newtonsoft.Json;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal class SetStatement : StatementBuilder, ISetStatement
    {
        public override string Template { get; } =" SET #props#";

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "props", "" }
        };

        protected Symbol Symbol { get; set; }
        
        public SetStatement(IStatementBuilder previous, Symbol symbol = null)
            :base(previous)
        {
            Symbol = symbol;
        }

        protected virtual void SetBody(Dictionary<string, object> value, Dictionary<string, object> overrides)
        {
            overrides = overrides ?? new Dictionary<string, object>();

            Variables["props"] = 
                string.Join(",",
                    value
                        .SelectPrimitiveTypesProperties()
                        .Select(kv =>
                            (Symbol!=null? $"{Symbol}.":"") +
                            $"{kv.Key}=" +
                            (overrides.ContainsKey(kv.Key) ?
                                overrides[kv.Key] :
                                    kv.Value == null ? "null" : JsonConvert.SerializeObject(kv.Value))));
        }

        public ISetStatement Body(object obj, object values = null)
        {
            obj = obj ?? throw new ArgumentNullException(nameof(obj));

            SetBody(obj.ToPropDictionary(), values?.ToPropDictionary());

            return this;
        }

        public ISetStatement Body<T>(T obj, object param = null) where T : class
        {
            obj = obj ?? throw new ArgumentNullException(nameof(obj));
            
            SetBody(obj.ToPropDictionary(), param?.ToPropDictionary());

            return this;
        }

        public ISetStatement Body<T>(Expression<Func<T, object>> expr, object values = null, object param = null) where T : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            Dictionary<string, object> tmp = values?.ToPropDictionary() ?? new Dictionary<string, object>();

            SetBody(
                typeof(T).GetProperties().ToDictionary(p => p.Name, p => (object)null).SelectProperties(expr.ToPropertyNameCollection())
                .ToDictionary(p => p.Key, p => tmp.ContainsKey(p.Key) ? tmp[p.Key] : null),
                param?.ToPropDictionary());

            return this;
        }
    }
}
