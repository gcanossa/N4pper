using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public class NodeExpressionBuilder<T> : TypedStatementBuilder<T> where T : class
    {
        public override string Template { get; } = "(#symbol##labels##body#)";

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "#symbol#", ""},
            { "#labels#", ""},
            { "#body#", ""}
        };

        protected HashSet<string> Labels { get; } = new HashSet<string>();
        protected EntityExpressionBodyBuilder<T> Body { get; } = new EntityExpressionBodyBuilder<T>();

        public NodeExpressionBuilder<T> AddLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) throw new ArgumentException("cannot be null or empty", nameof(label));

            Labels.Add(label);
            return this;
        }
        public NodeExpressionBuilder<T> SetSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) throw new ArgumentException("cannot be null or empty", nameof(symbol));

            Variables["#symbol#"] = symbol;

            return this;
        }

        public EntityExpressionBodyBuilder<T> WithBody()
        {
            return Body;
        }

        public override string Build()
        {
            Variables["#labels#"] = string.Join(":", Labels.ToArray());
            if (Variables["#labels#"] != "")
                Variables["#labels#"] = ":" + Variables["#labels#"];
            Variables["#body#"] = Body.Build();

            return base.Build();
        }
        public override void Reset()
        {
            Body.Reset();
            base.Reset();
        }
    }
}
