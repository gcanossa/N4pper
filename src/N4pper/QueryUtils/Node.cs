using System;
using System.Collections.Generic;
using OMnG;
using System.Text;
using System.Linq;

namespace N4pper.QueryUtils
{
    public class Node : EntityBase, INode
    {
        public Node(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null)
            :base(symbol, type, props)
        {
        }

        public virtual string Labels
        {
            get { return Type == null? "" : ":" + string.Join(":", TypeExtensions.GetLabels(Type).Select(p=> $"`{p}`")); }
        }

        public virtual INode SetSymbol(Symbol symbol)
        {
            Symbol = symbol;
            return this;
        }
        public virtual INode SetType(Type type)
        {
            Type = type;
            return this;
        }

        public override string Build()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");
            if (Symbol != null)
                sb.Append(Symbol);
            sb.Append(Labels);

            if (Props.Count > 0)
                sb.Append(SerializeProps());

            sb.Append(")");

            return sb.ToString();
        }

        public RelPath _(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null)
        {
            return new RelPath(this, new Rel(symbol, type, props),EdgeType.Any);
        }
        public RelPath V_(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null)
        {
            return new RelPath(this, new Rel(symbol, type, props), EdgeType.From);
        }
        public RelPath _(IRel rel)
        {
            return new RelPath(this, rel, EdgeType.Any);
        }

        public RelPath V_(IRel rel)
        {
            return new RelPath(this, rel, EdgeType.From);
        }
    }
}
