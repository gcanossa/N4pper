using Newtonsoft.Json;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal abstract class EntityStatementBuilder : StatementBuilder
    {
        public EntityStatementBuilder(IStatementBuilder previous) : base(previous)
        {
        }

        protected virtual void SetBody(Dictionary<string, object> value, Dictionary<string, object> overrides)
        {
            overrides = overrides ?? new Dictionary<string, object>();

            Variables["body"] = "{" +
                string.Join(",",
                    value
                        .SelectPrimitiveTypesProperties()
                        .Select(kv =>
                            $"{kv.Key}:" +
                            (overrides.ContainsKey(kv.Key) ?
                                overrides[kv.Key] :
                                    kv.Value == null ? "null" : JsonConvert.SerializeObject(kv.Value)))) +
                    "}";
        }
    }
}
