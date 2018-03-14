using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public abstract class StatementBuilder
    {
        public static implicit operator string(StatementBuilder obj)
        {
            return obj?.Build();
        }

        protected abstract IDictionary<string, string> Variables { get; }
        public abstract string Template { get; }

        protected string Statement { get; set; }

        public StatementBuilder()
        {
            Reset();
        }

        public virtual string Build()
        {
            Reset();

            Variables.Keys.ToList().ForEach(p => Statement = Statement.Replace(p, Variables[p]));

            return Statement;
        }

        public virtual void Reset()
        {
            Statement = Template;
        }

        public override string ToString()
        {
            return Build();
        }
    }
}
