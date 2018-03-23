using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public class NodePath : EntityBase, INode
    {
        public INode Node { get; protected set; }
        protected IRel Previous { get; set; }
        public EdgeType Edge { get; set; }

        internal NodePath(IRel previous, INode node, EdgeType edge = EdgeType.Any)
        {
            previous = previous ?? throw new ArgumentNullException(nameof(previous));
            node = node ?? throw new ArgumentNullException(nameof(node));

            Node = node;
            Previous = previous;
            Edge = edge;
        }

        public string Labels
        {
            get { return Node.Labels; }
        }

        public INode SetSymbol(Symbol symbol)
        {
            Node.SetSymbol(symbol);
            return this;
        }
        public INode SetType(Type type)
        {
            Node.SetType(type);
            return this;
        }
        public RelPath _(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null)
        {
            return new RelPath(this, new Rel(symbol, type, props), EdgeType.Any);
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

        public override string Build()
        {
            return $"{Previous}{Edge.ToCypherString()}{Node}";
        }
    }
}
