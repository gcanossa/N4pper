using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace N4pper.Ogm.Core
{
    public class RelEntity
    {
        public long SourceId { get; set; }
        public long DestinationId { get; set; }
        protected IOgmEntity Entity { get; set; }
        protected bool AllowNull { get; set; }
        public RelEntity(IOgmEntity entity, long sourceId, long destinationId, bool allowNull = true)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            SourceId = sourceId;
            DestinationId = destinationId;
            AllowNull = allowNull;
        }

        public string Label
        {
            get
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    return TypeExtensions.GetLabel(Entity.GetType());
                }
            }
        }
        public IDictionary<string, object> Properties
        {
            get
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    return Entity.ToPropDictionary().Where(p => (AllowNull || p.Value != null) && (p.Value?.IsPrimitive()??true)).ToDictionary(p => p.Key, p => p.Value);
                }
            }
        }
    }
}
