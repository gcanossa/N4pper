using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal class MergeStatement : StatementBuilder, IMergeStatement//TODO: fai dopo aver fatto la set
    {
        public override string Template { get; } = "MERGE #paths#";

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "paths", "" }
        };
        protected List<IStatementBuilder> Builders { get; } = new List<IStatementBuilder>();

        public MergeStatement(IStatementBuilder previous, INodePattern pattern, params INodePattern[] patterns)
            :base(previous)
        {
            Variables["paths"] = pattern.Build();
            if (patterns.Length > 0)
                Variables["paths"] += "," + string.Join(",", patterns.Select(p => p.Build()));
        }
        public override string Build()
        {
            return base.Build() + (Builders.Count == 0 ? "" : string.Join("", Builders.Select(p => p.Build())));
        }

        public IMergeOnMatchStatement OnMatch(Symbol symbol, Action<ISetStatement> statement)
        {
            SetStatement tmp = new SetStatement(null, symbol);
            Builders.Add(new StatementWrapper(" ON MATCH{0}", tmp));
            statement(tmp);

            return this;
        }

        public IMergeOnCreateStatement OnCreate(Symbol symbol, Action<ISetStatement> statement)
        {
            SetStatement tmp = new SetStatement(null, symbol);
            Builders.Add(new StatementWrapper(" ON CREATE{0}", tmp));
            statement(tmp);

            return this;
        }
    }
}
