using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Cypher.Impl
{
    internal class NodePattern : EntityStatementBuilder, INodePattern
    {
        public IEnumerable<Symbol> Symbols { get; protected set; }

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "symbol", "" },
            { "labels", "" },
            { "body", "" }
        };

        public override string Template { get; } = "(#symbol##labels##body#)";

        internal NodePattern(IPattern previous = null)
            :base(previous)
        {
            Symbols = previous?.Symbols?.ToList() ?? new List<Symbol>();
        }

        public INodePattern Node()
        {
            return this;
        }

        public INodePattern Node(object node, object values = null)
        {
            node = node ?? throw new ArgumentNullException(nameof(node));

            SetBody(node.ToPropDictionary(), values?.ToPropDictionary());

            return this;
        }

        public INodePattern Node<T>() where T : class
        {
            Variables["labels"] = $":{string.Join(":", typeof(T).GetLabels(TypeExtConf))}";

            SetBody(typeof(T).GetProperties().ToDictionary(p => p.Name, p => (object)null), null);

            return this;
        }

        public INodePattern Node<T>(T node, object param = null) where T : class
        {
            node = node ?? throw new ArgumentNullException(nameof(node));

            Variables["labels"] = $":{string.Join(":", typeof(T).GetLabels(TypeExtConf))}";

            SetBody(node.ToPropDictionary(), param?.ToPropDictionary());

            return this;
        }

        public INodePattern Node<T>(Expression<Func<T, object>> expr, object values = null, object param = null) where T : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            Variables["labels"] = $":{string.Join(":",typeof(T).GetLabels(TypeExtConf))}";

            Dictionary<string, object> tmp = values?.ToPropDictionary() ?? new Dictionary<string, object>();

            SetBody(
                typeof(T).GetProperties().ToDictionary(p => p.Name, p => (object)null).SelectProperties(expr.ToPropertyNameCollection())
                .ToDictionary(p=>p.Key, p=>tmp.ContainsKey(p.Key)?tmp[p.Key]:null), 
                param?.ToPropDictionary());

            return this;
        }

        public INodePattern Symbol(Symbol symbol)
        {
            if(!((List<Symbol>)Symbols).Contains(symbol))
                ((List<Symbol>)Symbols).Add(symbol);

            Variables["symbol"] = symbol;

            return this;
        }

        public INodePattern SetLabels(params string[] labels)
        {
            if (labels == null || labels.Length == 0)
                Variables["labels"] = "";
            else
                Variables["labels"] = ":"+string.Join(":", labels);

            return this;
        }

        public IRelPattern X_
        {
            get
            {
                return new RelPattern(new PatternWrapper("{0}<-", this));
            }
        }

        public IRelPattern _
        {
            get
            {
                return new RelPattern(new PatternWrapper("{0}-", this));
            }
        }
    }
}
