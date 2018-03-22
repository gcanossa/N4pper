using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OMnG;

namespace N4pper.QueryUtils
{
    internal class QueryBuilder : IQueryBuilder
    {
        private List<Symbol> _symbols = new List<Symbol>();
        public IEnumerable<Symbol> Symbols => _symbols;

        protected Dictionary<Symbol, INode> Nodes { get; set; } = new Dictionary<Symbol, INode>();
        protected Dictionary<Symbol, IRel> Rels { get; set; } = new Dictionary<Symbol, IRel>();

        public INode Node<T>(Symbol symbol, object param) where T : class
        {
            Dictionary<string, object> p = param?.ToPropDictionary()?.SelectPrimitiveTypesProperties();

            return Node<T>(symbol, p);
        }

        public INode Node<T>(Symbol symbol, Dictionary<string, object> param = null) where T : class
        {
            symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            param = param ?? new Dictionary<string, object>();

            if (Nodes.ContainsKey(symbol))
                throw new ArgumentException($"Node symbol already present '{symbol}'", nameof(symbol));
            if (Rels.ContainsKey(symbol))
                throw new ArgumentException($"Symbol already used for relationship '{symbol}'", nameof(symbol));

            Node n = new Node(symbol, typeof(T), param);

            Nodes.Add(symbol, n);
            _symbols.Add(symbol);
            return n;
        }

        public INode Node(Symbol symbol)
        {
            symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            return Nodes[symbol];
        }

        public IRel Rel<T>(Symbol symbol, object param) where T : class
        {
            Dictionary<string, object> p = param?.ToPropDictionary()?.SelectPrimitiveTypesProperties();

            return Rel<T>(symbol, p);
        }

        public IRel Rel<T>(Symbol symbol, Dictionary<string, object> param = null) where T : class
        {
            symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            param = param ?? new Dictionary<string, object>();

            if (Rels.ContainsKey(symbol))
                throw new ArgumentException($"Relationship symbol already present '{symbol}'", nameof(symbol));
            if (Nodes.ContainsKey(symbol))
                throw new ArgumentException($"Symbol already used for node '{symbol}'", nameof(symbol));

            Rel r = new Rel(symbol, typeof(T), param);

            Rels.Add(symbol, r);
            _symbols.Add(symbol);
            return r;
        }

        public IRel Rel(Symbol symbol)
        {
            symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            return Rels[symbol];
        }
        
        public Symbol Symbol(string name = null)
        {
            return new Symbol(name);
        }
    }
}
