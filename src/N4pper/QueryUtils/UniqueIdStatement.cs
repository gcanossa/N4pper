using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public class UniqueIdStatement : IStatementBuilder
    {
        public static implicit operator string(UniqueIdStatement statement)
        {
            return statement?.Build();
        }
        public Symbol Id { get; protected set; }
        public UniqueIdStatement(Symbol idSymbol)
        {
            Id = idSymbol ?? throw new ArgumentNullException(nameof(idSymbol));
        }
        public string Build()
        {
            Symbol tmp = new Symbol();
            return $"MERGE ({tmp}:{Constants.GlobalIdentityNodeLabel}) ON CREATE SET {tmp}.count = 1 ON MATCH SET {tmp}.count = {tmp}.count + 1 WITH {tmp}.count AS {Id}";
        }
        public override string ToString()
        {
            return Build();
        }
    }
}
