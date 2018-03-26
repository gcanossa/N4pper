using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace N4pper.Queryable.CypherSintaxHelpers
{
    public abstract class WithStatementModifier : CypherStatementModifier
    {
        protected Dictionary<string, List<Tuple<string, string>>> CurrentVariables { get; set; } = new Dictionary<string, List<Tuple<string, string>>>();

        public WithStatementModifier()
        {
        }

        public override string TokenizerRegexp => @"WITH\s+((?:(\*)|(?:[\w_\(\)]+\s+AS\s+([\w_\(\)]+))|([\w_\(\)]+))\s*,\s*)*((?:(\*)|(?:[\w_\(\)]+\s+AS\s+([\w_\(\)]+))|([\w_\(\)]+)))";

        public override CypherStatementModifier Tokenize(string cypherStatement)
        {
            CurrentVariables.Clear();
            base.Tokenize(cypherStatement);

            if (Tokens.Count == 0)
                return this;

            foreach (string item in Tokens.Keys)
            {
                CurrentVariables.Add(item, new List<Tuple<string, string>>());
            }
            
            int i = 0;
            string prevKey = null;
            foreach (string currKey in Tokens.Keys)
            {
                foreach (Match m in Regex.Matches(Tokens[currKey].Value.Substring(4), @"(?:(\*)|(?:[\w_\(\)]+\s+AS\s+([\w_\(\)]+))|([\w_\(\)]+))", RegexOptions.IgnoreCase))
                {
                    if (!string.IsNullOrEmpty(m.Groups[1].Value))
                    {
                        List<Tuple<string, string>> querySymbols = new List<Tuple<string, string>>();
                        foreach (Match mq in Regex.Matches(BaseStatement.Substring(
                            prevKey==null && Tokens[currKey].Index !=0 ? 0 : Tokens[prevKey].Index + Tokens[prevKey].Length, 
                            Tokens[currKey].Index - (Tokens[prevKey].Index + Tokens[prevKey].Length)
                            ), @"[\(\[]\s*([\w_]+)(?:\s*|:)", RegexOptions.IgnoreCase))
                        {
                            if (!querySymbols.Any(p => p.Item1 == mq.Groups[1].Value))
                                querySymbols.Add(new Tuple<string, string>(mq.Groups[1].Value, null));
                        }
                        CurrentVariables[currKey].AddRange(CurrentVariables[prevKey].Select(p => new Tuple<string, string>(p.Item1, null)));
                        CurrentVariables[currKey].AddRange(querySymbols);
                    }
                    else if (!string.IsNullOrEmpty(m.Groups[2].Value))
                    {
                        CurrentVariables[currKey].Add(new Tuple<string,string>(m.Groups[2].Value, m.Value));
                    }
                    else if (!string.IsNullOrEmpty(m.Groups[3].Value))
                    {
                        CurrentVariables[currKey].Add(new Tuple<string,string>(m.Groups[3].Value, null));
                    }
                    else
                        throw new Exception("Statement parsing error");
                }

                prevKey = currKey;
                i++;
            }

            return this;
        }
    }
}
