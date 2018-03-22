using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace N4pper.Queryable.CypherSintaxHelpers
{
    public abstract class CypherStatementModifier
    {
        public string BaseStatement { get; protected set; }
        public Dictionary<string, Match> Tokens { get; } = new Dictionary<string, Match>();
        protected List<string> TokenKeys { get { return Tokens.Keys.ToList(); } }
        
        public CypherStatementModifier()
        {}

        public abstract string TokenizerRegexp { get; }

        public virtual CypherStatementModifier Tokenize(string cypherStatement)
        {
            BaseStatement = cypherStatement ?? throw new ArgumentNullException(nameof(cypherStatement));
            Tokens.Clear();

            int i = 0;
            foreach (Match m in Regex.Matches(BaseStatement, TokenizerRegexp, RegexOptions.IgnoreCase))
            {
                Tokens.Add($"#{i++}#", m);
            }

            return this;
        }

        protected abstract string Modify(string tokenKey);

        public string Rebuild()
        {
            int i = 0;
            string tmp = Regex.Replace(BaseStatement, TokenizerRegexp, m => $"#{i++}#", RegexOptions.IgnoreCase);
            foreach (KeyValuePair<string, Match> kv in Tokens)
            {
                tmp = tmp.Replace(kv.Key, Modify(kv.Key));
            }

            return tmp;
        }
    }
}
