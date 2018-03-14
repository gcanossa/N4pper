using N4pper.Cypher.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Cypher
{
    public static class Cypr
    {
        public static Symbol Symbol(string value = null)
        {
            return new Symbol(value ?? $"_{Guid.NewGuid().ToString("N")}");
        }
        public static IPathClauseStatement Match(INodePattern pattern, params INodePattern[] patterns)
        {
            return new MatchStatement(null, pattern, patterns);
        }
        public static IPathClauseStatement OptionalMatch(INodePattern pattern, params INodePattern[] patterns)
        {
            return new OptionalMatchStatement(null, pattern, patterns);
        }
        public static IMergeStatement Merge(INodePattern pattern, params INodePattern[] patterns)
        {
            return new MergeStatement(null, pattern, patterns);
        }

        public static IPathClauseStatement Create(INodePattern pattern, params INodePattern[] patterns)
        {
            return new CreateStatement(null, pattern, patterns);
        }

        public static INodePattern Path()
        {
            return new NodePattern();
        }

        private static Random RndGen = new Random();
        private static (int, long) GetSlot()
        {
            int tmp = RndGen.Next(Int32.MaxValue - 1);
            return (tmp, (Int64.MaxValue / Int32.MaxValue) * tmp);
        }
        public static IStatementBuilder UniqueId(Symbol idSymbol)
        {
            idSymbol = idSymbol ?? throw new ArgumentNullException(nameof(idSymbol));

            (int slot, long baseCount) = GetSlot();

            return new RawStatement($"MERGE (id:{Constants.GlobalIdentityNodeLabel} {{slot:{slot}}}) ON CREATE SET id.count = {baseCount} ON MATCH SET id.count = id.count + 1 WITH id.count AS {idSymbol}");
        }
    }
}
