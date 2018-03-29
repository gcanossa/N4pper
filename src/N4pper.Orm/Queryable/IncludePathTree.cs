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

            ITree<IncludePathComponent> t = Branches.FirstOrDefault(p => p.Equals(tree));
            if (t == null)
            {
                t = tree;

                Branches.Add(tree);
            }

            return t;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
                return ((IncludePathTree)obj).Item.SourceProperty == Item.SourceProperty && ((IncludePathTree)obj).Item.DestinationProperty == Item.DestinationProperty;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
