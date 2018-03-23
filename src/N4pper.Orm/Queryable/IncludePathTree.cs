using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Queryable
{
    internal class IncludePathTree
    {
        public IncludePathComponent Path { get; set; }
        public List<IncludePathTree> Branches { get; } = new List<IncludePathTree>();

        public IncludePathTree Add(IncludePathTree tree)
        {
            tree = tree ?? throw new ArgumentNullException(nameof(tree));

            IncludePathTree t = Branches.FirstOrDefault(p => p.Path.Property.Equals(tree.Path.Property));
            if (t == null)
            {
                t = tree;

                Branches.Add(tree);
            }

            return t;
        }
    }
}
