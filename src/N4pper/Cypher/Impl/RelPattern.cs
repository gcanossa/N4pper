using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using OMnG;

namespace N4pper.Cypher.Impl
{
    internal class RelPattern : EntityStatementBuilder, IRelPattern
    {
        public IEnumerable<Symbol> Symbols { get; protected set; }

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "symbol", "" },
            { "type", "" },
            { "numbers", "" },
            { "body", "" }
        };

        public override string Template { get; } = "[#symbol##type##numbers##body#]";

        public override string Build()
        {
            string t = base.Build();
            return t.EndsWith("[]") ? t.Substring(0,t.Length-2): t;
        }

        internal RelPattern(IPattern previous)
            :base(previous)
        {
            Symbols = previous.Symbols?.ToList() ?? new List<Symbol>();
        }

        public IRelPattern Rel()
        {
            return this;
        }
        
        public IRelPattern Rel(object rel, object values = null)
        {
            rel = rel ?? throw new ArgumentNullException(nameof(rel));

            SetBody(rel.ToPropDictionary(), values?.ToPropDictionary());

            return this;
        }

        public IRelPattern Rel<T>() where T : class
        {
            Variables["type"] = $":{typeof(T).GetLabel(TypeExtConf)}";

            SetBody(typeof(T).GetProperties().ToDictionary(p=>p.Name, p=>(object)null), null);

            return this;
        }

        public IRelPattern Rel<T>(T rel, object param = null) where T : class
        {
            rel = rel ?? throw new ArgumentNullException(nameof(rel));

            Variables["type"] = $":{typeof(T).GetLabel(TypeExtConf)}";

            SetBody(rel.ToPropDictionary(), param?.ToPropDictionary());

            return this;
        }

        public IRelPattern Rel<T>(Expression<Func<T, object>> expr, object values = null, object param = null) where T : class
        {
            expr = expr ?? throw new ArgumentNullException(nameof(expr));

            Variables["type"] = $":{typeof(T).GetLabel(TypeExtConf)}";

            Dictionary<string, object> tmp = values?.ToPropDictionary() ?? new Dictionary<string, object>();

            SetBody(
                typeof(T).GetProperties().ToDictionary(p => p.Name, p => (object)null).SelectProperties(expr.ToPropertyNameCollection())
                .ToDictionary(p => p.Key, p => tmp.ContainsKey(p.Key) ? tmp[p.Key] : null),
                param?.ToPropDictionary());

            return this;
        }
        public IRelPattern PathLength(int? min = null, int? max = null)
        {
            Variables["numbers"] = 
                "*" + 
                (min?.ToString()??"") + 
                (min != null || max != null ? ".." : "") +
                (max?.ToString()??"");

            return this;
        }

        public IRelPattern Symbol(Symbol symbol)
        {
            if (!((List<Symbol>)Symbols).Contains(symbol))
                ((List<Symbol>)Symbols).Add(symbol);

            Variables["symbol"] = symbol;

            return this;
        }

        public IRelPattern SetType(string type)
        {
            Variables["type"] = !string.IsNullOrEmpty(type) ? ":" + type : "";

            return this;
        }

        public INodePattern _X
        {
            get
            {
                return new NodePattern(new PatternWrapper("{0}->", this));
            }
        }

        public INodePattern _
        {
            get
            {
                return new NodePattern(new PatternWrapper("{0}-", this));
            }
        }
    }
}
