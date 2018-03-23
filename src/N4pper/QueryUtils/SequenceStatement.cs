using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public class SequenceStatement : IStatementBuilder
    {
        public static implicit operator string(SequenceStatement statement)
        {
            return statement?.Build();
        }
        public Symbol Id { get; protected set; }
        public string SequenceName { get; protected set; }
        public SequenceStatement(string sequenceName, Symbol idSymbol)
        {
            Id = idSymbol ?? throw new ArgumentNullException(nameof(idSymbol));
            SequenceName = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));
        }
        public string Build()
        {
            Symbol tmp = new Symbol();
            return $"MERGE ({tmp}:{SequenceName}) ON CREATE SET {tmp}.count = 1 ON MATCH SET {tmp}.count = {tmp}.count + 1 WITH {tmp}.count AS {Id}";
        }
        public string BuildForQuery()
        {
            return Build();
        }
        public override string ToString()
        {
            return Build();
        }
    }
}
