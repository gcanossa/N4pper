using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher.Impl
{
    public class RawStatement : IStatementBuilder
    {
        public IStatementBuilder Previous => null;

        protected string Statement { get; set; }

        public RawStatement(string statement)
        {
            statement = statement ?? throw new ArgumentNullException(nameof(statement));

            Statement = statement;
        }

        public string Build()
        {
            return Statement;
        }
    }
}
