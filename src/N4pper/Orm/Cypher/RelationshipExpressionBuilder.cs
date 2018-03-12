using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public class RelationshipExpressionBuilder<T> : TypedStatementBuilder<T> where T : class
    {
        public override string Template { get; } = "[#symbol##label##body#]";

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "#symbol#", ""},
            { "#label#", ""},
            { "#body#", ""}
        };

        protected EntityExpressionBodyBuilder<T> Body { get; } = new EntityExpressionBodyBuilder<T>();

        public RelationshipExpressionBuilder<T> SetLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) throw new ArgumentException("cannot be null or empty", nameof(label));

            Variables["#label#"] = ":"+label;

            return this;
        }
        public RelationshipExpressionBuilder<T> SetSymbol(string symbol)
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
