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
        ITree<T> Add(ITree<T> tree);
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

        public static void BreathFirstLevels<T>(this ITree<T> ext, Action<IEnumerable<ITree<T>>> reduce, bool rootToLeaf = true, bool leftToRight = true)
        {
            if (ext == null) return;

            if (rootToLeaf)
                reduce(new List<ITree<T>>() { ext });

            (ext.Branches ?? new List<ITree<T>>()).BreathFirstLevelsStep(reduce, rootToLeaf, leftToRight);

            if (!rootToLeaf)
                reduce(new List<ITree<T>>() { ext });
        }
        public static void BreathFirstLevelsStep<T>(this IEnumerable<ITree<T>> ext, Action<IEnumerable<ITree<T>>> reduce, bool rootToLeaf = true, bool leftToRight = true)
        {
            if (ext == null || ext.Count() == 0) return;

            IEnumerable<ITree<T>> items = (leftToRight ?
                ext : ext?.Reverse()) ?? new List<ITree<T>>();

            if (rootToLeaf)
                reduce(items);

            (ext?.SelectMany(p => p.Branches) ?? new List<ITree<T>>()).BreathFirstLevelsStep(reduce, rootToLeaf, leftToRight);

            if (!rootToLeaf)
                reduce(items);
        }
        public static void DepthFirst<T>(this ITree<T> ext, Action<ITree<T>> reduce, bool rootToLeaf = true, bool leftToRight = true)
        {
            if (ext == null) return;

            if (rootToLeaf)
                reduce(ext);

            IEnumerable<ITree<T>> branches = (leftToRight ?
                ext.Branches : ext.Branches?.AsEnumerable()?.Reverse()) ?? new List<ITree<T>>();

            foreach (ITree<T> branch in branches)
                    branch.DepthFirst(reduce, rootToLeaf, leftToRight);

            if (!rootToLeaf)
                reduce(ext);
        }
        public static void DepthFirstPaths<T>(this ITree<T> ext, Action<ITree<T>, ITree<T>> reduce, bool rootToLeaf = true, bool leftToRight = true)
        {
            if (ext == null || ext.Branches.Count==0) return;

            IEnumerable<ITree<T>> branches = (leftToRight ?
                ext.Branches : ext.Branches?.AsEnumerable()?.Reverse()) ?? new List<ITree<T>>();

            foreach (ITree<T> item in branches)
            {
                if (rootToLeaf)
                {
                    reduce(ext, item);
                    item.DepthFirstPaths(reduce, rootToLeaf, leftToRight);
                }
                else
                {
                    item.DepthFirstPaths(reduce, rootToLeaf, leftToRight);
                    reduce(ext, item);
                }
            }
        }
    }
}
