using N4pper.Ogm.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTest
{
    public class TreeUtilsTests
    {
        #region nested types

        public class IntTree : ITree<int>
        {
            public int Item { get; set; }
            public List<ITree<int>> Branches { get; private set; } = new List<ITree<int>>();

            public ITree<int> Add(ITree<int> tree)
            {
                ITree<int> t = Branches.FirstOrDefault(p=>p==tree);
                if (t == null)
                {
                    t = tree;
                    Branches.Add(tree);
                }
                return t;
            }
        }
        #endregion

        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstRootLeft))]
        public void DepthFirstRootLeft()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 1 };
            IntTree right = new IntTree() { Item = 4 };
            left.Add(new IntTree() { Item = 2 });
            left.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 6 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.DepthFirst(t => values.Add(t.Item));

            Assert.Equal(new int[] { 0,1,2,3,4,5,6 }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstRootRight))]
        public void DepthFirstRootRight()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 4 };
            IntTree right = new IntTree() { Item = 1 };
            left.Add(new IntTree() { Item = 6 });
            left.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 2 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.DepthFirst(t => values.Add(t.Item),true, false);

            Assert.Equal(new int[] { 0, 1, 2, 3, 4, 5, 6 }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstLeafLeft))]
        public void DepthFirstLeafLeft()
        {
            IntTree root = new IntTree() { Item = 6 };
            IntTree left = new IntTree() { Item = 2 };
            IntTree right = new IntTree() { Item = 5 };
            left.Add(new IntTree() { Item = 0 });
            left.Add(new IntTree() { Item = 1 });
            right.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 4 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.DepthFirst(t => values.Add(t.Item), false);

            Assert.Equal(new int[] { 0, 1, 2, 3, 4, 5, 6 }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstLeafRight))]
        public void DepthFirstLeafRight()
        {
            IntTree root = new IntTree() { Item = 6 };
            IntTree left = new IntTree() { Item = 5 };
            IntTree right = new IntTree() { Item = 2 };
            left.Add(new IntTree() { Item = 4 });
            left.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 1 });
            right.Add(new IntTree() { Item = 0 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.DepthFirst(t => values.Add(t.Item), false, false);

            Assert.Equal(new int[] { 0, 1, 2, 3, 4, 5, 6 }, values);
        }


        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstPathsRootLeft))]
        public void DepthFirstPathsRootLeft()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 1 };
            IntTree right = new IntTree() { Item = 4 };
            left.Add(new IntTree() { Item = 2 });
            left.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 6 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.DepthFirstPaths((p,t) => values.Add($"{p.Item}-{t.Item}"));

            Assert.Equal(new string[] { "0-1", "1-2", "1-3", "0-4", "4-5", "4-6" }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstPathsRootRight))]
        public void DepthFirstPathsRootRight()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 4 };
            IntTree right = new IntTree() { Item = 1 };
            left.Add(new IntTree() { Item = 6 });
            left.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 2 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.DepthFirstPaths((p, t) => values.Add($"{p.Item}-{t.Item}"), true, false);

            Assert.Equal(new string[] { "0-1", "1-2", "1-3", "0-4", "4-5", "4-6" }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstPathsLeafLeft))]
        public void DepthFirstPathsLeafLeft()
        {
            IntTree root = new IntTree() { Item = 6 };
            IntTree left = new IntTree() { Item = 2 };
            IntTree right = new IntTree() { Item = 5 };
            left.Add(new IntTree() { Item = 0 });
            left.Add(new IntTree() { Item = 1 });
            right.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 4 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.DepthFirstPaths((p, t) => values.Add($"{p.Item}-{t.Item}"), false);

            Assert.Equal(new string[] { "2-0", "2-1", "6-2", "5-3", "5-4", "6-5" }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(DepthFirstPathsLeafRight))]
        public void DepthFirstPathsLeafRight()
        {
            IntTree root = new IntTree() { Item = 6 };
            IntTree left = new IntTree() { Item = 5 };
            IntTree right = new IntTree() { Item = 2 };
            left.Add(new IntTree() { Item = 4 });
            left.Add(new IntTree() { Item = 3 });
            right.Add(new IntTree() { Item = 1 });
            right.Add(new IntTree() { Item = 0 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.DepthFirstPaths((p, t) => values.Add($"{p.Item}-{t.Item}"), false, false);

            Assert.Equal(new string[] { "2-0", "2-1", "6-2", "5-3", "5-4", "6-5" }, values);
        }

        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstRootLeft))]
        public void BreathFirstRootLeft()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 1 };
            IntTree right = new IntTree() { Item = 2 };
            left.Add(new IntTree() { Item = 3 });
            left.Add(new IntTree() { Item = 4 });
            right.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 6 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.BreathFirst(t => values.Add(t.Item));

            Assert.Equal(new int[] { 0, 1, 2, 3, 4, 5, 6 }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstRootRight))]
        public void BreathFirstRootRight()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 2 };
            IntTree right = new IntTree() { Item = 1 };
            left.Add(new IntTree() { Item = 6 });
            left.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 4 });
            right.Add(new IntTree() { Item = 3 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.BreathFirst(t => values.Add(t.Item), true, false);

            Assert.Equal(new int[] { 0, 1, 2, 3, 4, 5, 6 }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstLeafLeft))]
        public void BreathFirstLeafLeft()
        {
            IntTree root = new IntTree() { Item = 6 };
            IntTree left = new IntTree() { Item = 4 };
            IntTree right = new IntTree() { Item = 5 };
            left.Add(new IntTree() { Item = 0 });
            left.Add(new IntTree() { Item = 1 });
            right.Add(new IntTree() { Item = 2 });
            right.Add(new IntTree() { Item = 3 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.BreathFirst(t => values.Add(t.Item), false);

            Assert.Equal(new int[] { 0, 1, 2, 3, 4, 5, 6 }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstLeafRight))]
        public void BreathFirstLeafRight()
        {
            IntTree root = new IntTree() { Item = 6 };
            IntTree left = new IntTree() { Item = 5 };
            IntTree right = new IntTree() { Item = 4 };
            left.Add(new IntTree() { Item = 3 });
            left.Add(new IntTree() { Item = 2 });
            right.Add(new IntTree() { Item = 1 });
            right.Add(new IntTree() { Item = 0 });
            root.Add(left);
            root.Add(right);

            List<int> values = new List<int>();

            root.BreathFirst(t => values.Add(t.Item), false, false);

            Assert.Equal(new int[] { 0, 1, 2, 3, 4, 5, 6 }, values);
        }

        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstLevelsRootLeft))]
        public void BreathFirstLevelsRootLeft()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 1 };
            IntTree right = new IntTree() { Item = 2 };
            left.Add(new IntTree() { Item = 3 });
            left.Add(new IntTree() { Item = 4 });
            right.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 6 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.BreathFirstLevels(t => values.Add(string.Join("-",t.Select(p=>p.Item))));

            Assert.Equal(new string[] { "0", "1-2", "3-4-5-6" }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstLevelsRootRight))]
        public void BreathFirstLevelsRootRight()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 1 };
            IntTree right = new IntTree() { Item = 2 };
            left.Add(new IntTree() { Item = 3 });
            left.Add(new IntTree() { Item = 4 });
            right.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 6 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.BreathFirstLevels(t => values.Add(string.Join("-", t.Select(p => p.Item))), true, false);

            Assert.Equal(new string[] { "0", "2-1", "6-5-4-3" }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstLevelsLeafLeft))]
        public void BreathFirstLevelsLeafLeft()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 1 };
            IntTree right = new IntTree() { Item = 2 };
            left.Add(new IntTree() { Item = 3 });
            left.Add(new IntTree() { Item = 4 });
            right.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 6 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.BreathFirstLevels(t => values.Add(string.Join("-", t.Select(p => p.Item))), false);

            Assert.Equal(new string[] { "3-4-5-6", "1-2", "0" }, values);
        }
        [Trait("Category", nameof(TreeUtilsTests))]
        [Fact(DisplayName = nameof(BreathFirstLevelsLeafRight))]
        public void BreathFirstLevelsLeafRight()
        {
            IntTree root = new IntTree() { Item = 0 };
            IntTree left = new IntTree() { Item = 1 };
            IntTree right = new IntTree() { Item = 2 };
            left.Add(new IntTree() { Item = 3 });
            left.Add(new IntTree() { Item = 4 });
            right.Add(new IntTree() { Item = 5 });
            right.Add(new IntTree() { Item = 6 });
            root.Add(left);
            root.Add(right);

            List<string> values = new List<string>();

            root.BreathFirstLevels(t => values.Add(string.Join("-", t.Select(p => p.Item))), false, false);

            Assert.Equal(new string[] { "6-5-4-3", "2-1", "0" }, values);
        }
    }
}
