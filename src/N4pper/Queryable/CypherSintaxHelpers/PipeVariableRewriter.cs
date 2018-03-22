using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace N4pper.Queryable.CypherSintaxHelpers
{
    public class PipeVariableRewriter : WithStatementModifier
    {
        public PipeVariableRewriter()
        {
        }

        protected override string Modify(string tokenKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("WITH ");

            for (int i = 0; i < CurrentVariables[tokenKey].Count; i++)
            {
                if(CurrentVariables[tokenKey][i].Item2!=null)
                {
                    sb.Append(CurrentVariables[tokenKey][i].Item2);
                }
                else if(Regex.IsMatch(CurrentVariables[tokenKey][i].Item1, @"[^\s\w_]", RegexOptions.IgnoreCase))
                {
                    sb.Append(CurrentVariables[tokenKey][i].Item1);
                    sb.Append(" AS ");
                    string s = $"_{Guid.NewGuid().ToString("N")}";
                    sb.Append(s);

                    for (int j = TokenKeys.IndexOf(tokenKey)+1; j < TokenKeys.Count; j++)
                    {
                        int idx=CurrentVariables[TokenKeys[j]].IndexOf(CurrentVariables[tokenKey][i]);
                        CurrentVariables[TokenKeys[j]][idx] = new Tuple<string, string>(s, null);
                    }
                }
                else
                {
                    sb.Append(CurrentVariables[tokenKey][i].Item1);
                }
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
