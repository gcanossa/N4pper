using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.QueryUtils
{
    public enum EdgeType
    {
        To,
        From,
        Any
    }

    public static class EdgeTypeExtensions
    {
        public static string ToCypherString(this EdgeType ext)
        {
            return ext == EdgeType.Any ? "-" : ext == EdgeType.To ? "->" : "<-";
        }
        public static EdgeType FromCypherString(this string ext)
        {
            return ext == "-" ? EdgeType.Any : ext == "->" ? EdgeType.To : ext == "<-" ? EdgeType.From : throw new ArgumentException("unknown edge type.", nameof(ext));
        }
    }
}
