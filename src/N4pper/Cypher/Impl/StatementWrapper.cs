using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal class StatementWrapper : IStatementBuilder
    {
        public StatementWrapper(string wrappingString, IStatementBuilder previous)
        {
            WrappingString = wrappingString;
            Previous = previous;
        }

        public IStatementBuilder Previous { get; set; }

        protected string WrappingString { get; set; }
        
        public virtual string Build()
        {
            return string.Format(WrappingString??"{0}", Previous.Build());
        }
    }
}
