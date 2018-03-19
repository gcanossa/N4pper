using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public class RelPath : EntityBase, IRel
    {
        public IRel Rel { get; protected set; }
        protected INode Previous { get; set; }
        public EdgeType Edge { get; set; }

        internal RelPath(INode previous, IRel rel, EdgeType edge = EdgeType.Any)
        {
            previous = previous ?? throw new ArgumentNullException(nameof(previous));
            rel = rel ?? throw new ArgumentNullException(nameof(rel));

            Rel = rel;
            Previous = previous;
            Edge = edge;
        }
        public IRel Lengths(int? min = null, int? max = null)
        {
            Rel.Lengths(min, max);

            return this;
        }

        public string Label
        {
            get { return Rel.Label; }
        }

        public IRel SetSymbol(Symbol symbol)
        {
            Rel.SetSymbol(symbol);
            return this;
        }
        public IRel SetType(Type type)
        {
            Rel.SetType(type);
            return this;
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

        public override string Build()
        {
            return $"{Previous}{Edge.ToCypherString()}{Rel}";
        }
    }
}
