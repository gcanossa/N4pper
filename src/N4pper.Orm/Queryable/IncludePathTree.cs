using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Queryable
{
    internal class IncludePathTree : ITree<IncludePathComponent>
    {
        public IncludePathComponent Item { get; set; }
        public List<ITree<IncludePathComponent>> Branches { get; } = new List<ITree<IncludePathComponent>>();
        
        public ITree<IncludePathComponent> Add(ITree<IncludePathComponent> tree)
        {
            tree = tree ?? throw new ArgumentNullException(nameof(tree));

            ITree<IncludePathComponent> t = Branches.FirstOrDefault(p => p.Item.Property.Equals(tree.Item.Property));
            if (t == null)
            {
                t = tree;

                Branches.Add(tree);
            }

            return t;
        }
    }
}
