using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.QueryUtils
{
    public class Rel : EntityBase, IRel
    {
        public Rel(Symbol symbol = null, Type type = null, Dictionary<string, object> props = null)
            : base(symbol, type, props)
        {
        }

        protected string PathLength { get; set; }
        public virtual IRel Lengths(int? min = null, int? max = null)
        {
            PathLength =
                "*" +
                (min?.ToString() ?? "") +
                (min != null || max != null ? ".." : "") +
                (max?.ToString() ?? "");

            return this;
        }

        public virtual string Label
        {
            get { return Type == null ? "" : $":`{TypeExtensions.GetLabel(Type)}`"; }
        }

        public virtual IRel SetSymbol(Symbol symbol)
        {
            Symbol = symbol;
            return this;
        }
        public virtual IRel SetType(Type type)
        {
            Type = type;
            return this;
        }
        public override string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            if (Symbol != null)
                sb.Append(Symbol);

            sb.Append(Label);

            if (PathLength != null)
                sb.Append(PathLength);
            if (Props.Count > 0)
                sb.Append(SerializeProps());

            sb.Append("]");

            return sb.ToString();
        }

        public NodePath _(Symbol symbol = null, Type type = null, Dictionary<string, object> props = null)
        {
            return new NodePath(this, new Node(symbol, type, props), EdgeType.Any);
        }
        public NodePath _V(Symbol symbol = null, Type type = null, Dictionary<string, object> props = null)
        {
            return new NodePath(this, new Node(symbol, type, props), EdgeType.To);
        }
        public NodePath _(INode node)
        {
            return new NodePath(this, node, EdgeType.Any);
        }

        public NodePath _V(INode node)
        {
            return new NodePath(this, node, EdgeType.To);
        }
    }
}
