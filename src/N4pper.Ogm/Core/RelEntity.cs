using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace N4pper.Ogm.Core
{
    public class RelEntity : EntityPlaceholder
    {
        public long SourceId { get; set; }
        public long DestinationId { get; set; }
        public RelEntity(IOgmEntity entity, long sourceId, long destinationId, bool allowNull = true, IEnumerable<string> excludePorperties = null)
            : base (entity, allowNull, excludePorperties)
        {
            SourceId = sourceId;
            DestinationId = destinationId;
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
    }
}
