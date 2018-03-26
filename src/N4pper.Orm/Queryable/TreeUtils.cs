using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Queryable
{
    public interface ITree<T>
    {
        T Item { get; set; }
        List<ITree<T>> Branches { get; }
        void Add(ITree<T> tree);
    }
    public static class TreeUtils
    {
        public static void BreathFirst<T>(this ITree<T> ext, Action<ITree<T>> reduce, bool rootToLeaf = true, bool leftToRight = true)
        {
            if (ext == null) return;

            if (rootToLeaf)
                reduce(ext);

            (ext.Branches ?? new List<ITree<T>>()).BreathFirstStep(reduce, rootToLeaf, leftToRight);

            if (!rootToLeaf)
                reduce(ext);
        }
        public static void BreathFirstStep<T>(this IEnumerable<ITree<T>> ext, Action<ITree<T>> reduce, bool rootToLeaf = true, bool leftToRight = true)
        {
            if (ext == null || ext.Count()==0) return;

            IEnumerable<ITree<T>> items = (leftToRight ?
                ext : ext?.Reverse()) ?? new List<ITree<T>>();

            if (rootToLeaf)
                foreach (ITree<T> item in items)
                    reduce(item);

            (ext?.SelectMany(p => p.Branches) ?? new List<ITree<T>>()).BreathFirstStep(reduce, rootToLeaf, leftToRight);
            
            if (!rootToLeaf)
                foreach (ITree<T> item in items)
                    reduce(item);
        }
        public static void DepthFirst<T>(this ITree<T> ext, Action<ITree<T>> reduce, bool rootToLeaf = true, bool leftToRight = true)
        {
            if (ext == null) return;

            if (rootToLeaf)
                reduce(ext);

            IEnumerable<ITree<T>> branches = (leftToRight ?
                ext.Branches : ext.Branches.AsEnumerable()?.Reverse()) ?? new List<ITree<T>>();

            foreach (ITree<T> branch in branches)
                    branch.DepthFirst(reduce, rootToLeaf, leftToRight);

            if (!rootToLeaf)
                reduce(ext);
        }
    }
}
