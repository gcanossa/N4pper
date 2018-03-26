using N4pper.Orm.Queryable;
using System;
using System.Collections.Generic;
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

            public void Add(ITree<int> tree)
            {
                if(!Branches.Contains(tree))
                    Branches.Add(tree);
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
    }
}
