using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Cypher
{
    public abstract class StatementBuilder : IStatementBuilder
    {
        protected static TypeExtensionsConfiguration TypeExtConf { get; } = new N4pperTypeExtensionsConfiguration();
        public static implicit operator string(StatementBuilder obj)
        {
            return obj?.Build();
        }
        public IStatementBuilder Previous { get; protected set; }

        protected abstract IDictionary<string, string> Variables { get; }
        public abstract string Template { get; }

        public StatementBuilder(IStatementBuilder previous)
        {
            Previous = previous;
        }

        public virtual string Build()
        {
            string tmp = Template;
            Variables.Keys.ToList().ForEach(p => tmp = tmp.Replace($"#{p}#", Variables[p]));

            return (Previous != null ? Previous.Build() : "") + tmp;
        }
        
        public override string ToString()
        {
            return Build();
        }
    }
}
