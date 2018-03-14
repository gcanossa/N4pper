using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal class PatternWrapper : StatementWrapper, IPattern
    {
        public PatternWrapper(string wrappingString, IPattern previous)
            :base(wrappingString, previous)
        {
            WrappingString = wrappingString;
            Previous = previous;
        }
        
        public IEnumerable<Symbol> Symbols => ((IPattern)Previous)?.Symbols;
    }
}
