using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Ogm.Core
{
    public class NodeEntity : EntityPlaceholder
    {
        public NodeEntity(IOgmEntity entity, bool allowNull = true, IEnumerable<string> excludePorperties = null)
            :base (entity, allowNull, excludePorperties)
        {
        }

        public List<string> Labels
        {
            get
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    return TypeExtensions.GetLabels(Entity.GetType()).ToList();
                }
            }
        }
    }
}
