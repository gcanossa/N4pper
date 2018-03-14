using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal class CreateStatement : PathClauseStatement
    {
        public override string Template { get; } = "CREATE #paths#";

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "paths", "" }
        };

        public CreateStatement(IStatementBuilder previous, INodePattern pattern, params INodePattern[] patterns)
            : base(previous)
        {
            Variables["paths"] = pattern.Build();
            if (patterns.Length > 0)
                Variables["paths"] += "," + string.Join(",", patterns.Select(p => p.Build()));
        }
    }
}
