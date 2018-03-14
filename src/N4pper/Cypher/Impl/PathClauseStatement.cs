using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal abstract class PathClauseStatement : StatementBuilder, IPathClauseStatement
    {
        public PathClauseStatement(IStatementBuilder previous) : base(previous)
        {
        }

        protected List<IStatementBuilder> Builders { get; } = new List<IStatementBuilder>();

        public IPathClauseStatement Set(Symbol symbol, Action<ISetStatement> statement)
        {
            SetStatement tmp = new SetStatement(null, symbol);
            Builders.Add(tmp);
            statement(tmp);

            return this;
        }

        public override string Build()
        {
            return base.Build() + (Builders.Count==0?"":string.Join("",Builders.Select(p=>p.Build())));
        }
    }
}
