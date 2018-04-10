using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Ogm.Core
{
    public abstract class EntityPlaceholder
    {
        protected IOgmEntity Entity { get; set; }
        protected bool AllowNull { get; set; }
        protected IEnumerable<string> ExcludedPorperties { get; set; }
        public EntityPlaceholder(IOgmEntity entity, bool allowNull = true, IEnumerable<string> excludePorperties = null)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            AllowNull = allowNull;
            ExcludedPorperties = excludePorperties ?? new string[0];
        }
        public long? EntityId => Entity.EntityId;
        public virtual IDictionary<string, object> Properties
        {
            get
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    return Entity.ToPropDictionary().ExludeProperties(ExcludedPorperties)
                        .Where(p => (AllowNull || p.Value != null) && (p.Value?.IsPrimitive() ?? true)).ToDictionary(p => p.Key, p => p.Value);
                }
            }
        }
    }
}
