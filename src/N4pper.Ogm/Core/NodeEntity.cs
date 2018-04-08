using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Ogm.Core
{
    public class NodeEntity
    {
        protected IOgmEntity Entity { get; set; }
        protected bool AllowNull { get; set; }
        public NodeEntity(IOgmEntity entity, bool allowNull = true)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            AllowNull = allowNull;
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
        public IDictionary<string, object> Properties
        {
            get
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    return Entity.ToPropDictionary().Where(p => (AllowNull || p.Value != null) && (p.Value?.IsPrimitive() ?? true)).ToDictionary(p => p.Key, p => p.Value);
                }
            }
        }
    }
}
