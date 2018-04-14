using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace N4pper.Ogm.Core
{
    public class ConnectionEntity : EntityPlaceholder
    {
        public long SourceId { get; set; }
        public long DestinationId { get; set; }

        protected long Version { get; set; }
        public ConnectionEntity(IOgmConnection entity, long sourceId, long destinationId, long version, bool allowNull = true, IEnumerable<string> excludePorperties = null)
            : base(entity, allowNull, excludePorperties)
        {
            SourceId = sourceId;
            DestinationId = destinationId;

            Version = version;
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
        public override  IDictionary<string, object> Properties
        {
            get
            {
                using (ManagerAccess.Manager.ScopeOMnG())
                {
                    IDictionary<string, object> tmp = base.Properties;
                    if (!tmp.ContainsKey(nameof(OgmConnection.Version)))
                        tmp.Add(nameof(OgmConnection.Version), Version);
                    else
                        tmp[nameof(OgmConnection.Version)] = Version;

                    return tmp;
                }
            }
        }
    }
}
