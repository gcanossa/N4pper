using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Ogm.Core
{
    public abstract class ChangeTrackerBase
    {
        protected List<EntityChangeDescriptor> ChangeLog { get; } = new List<EntityChangeDescriptor>();

        public virtual IEnumerable<EntityChangeDescriptor> GetChangeLog()
        {
            return ChangeLog;
        }

        public void Clear()
        {
            ChangeLog.Clear();
        }

        public void Untrack(IOgmEntity entity)
        {
            ChangeLog.RemoveAll(p => p.Entity == entity);
            ChangeLog.RemoveAll(p => p is EntityChangeRelCreation && (((EntityChangeRelCreation)p).Source == entity || ((EntityChangeRelCreation)p).Destination == entity));
        }
        public void Track(EntityChangeDescriptor item)
        {
            item = item ?? throw new ArgumentNullException(nameof(item));

            if (item is EntityChangeNodeCreation)
                NodeCreationChange(item as EntityChangeNodeCreation);
            else if (item is EntityChangeNodeDeletion)
                NodeDeletionChange(item as EntityChangeNodeDeletion);
            else if (item is EntityChangeNodeUpdate)
                NodeUpdateChange(item as EntityChangeNodeUpdate);
            else if (item is EntityChangeRelCreation)
                RelCreationChange(item as EntityChangeRelCreation);
            else if (item is EntityChangeRelDeletion)
                RelDeletionChange(item as EntityChangeRelDeletion);
            else if (item is EntityChangeRelUpdate)
                RelUpdateChange(item as EntityChangeRelUpdate);
            else
                throw new ArgumentException($"Invalid EntityChangeDescriptor type. {item.GetType().FullName}", nameof(item));
        }

        protected abstract void NodeCreationChange(EntityChangeNodeCreation item);
        protected abstract void NodeDeletionChange(EntityChangeNodeDeletion item);
        protected abstract void NodeUpdateChange(EntityChangeNodeUpdate item);
        protected abstract void RelCreationChange(EntityChangeRelCreation item);
        protected abstract void RelDeletionChange(EntityChangeRelDeletion item);
        protected abstract void RelUpdateChange(EntityChangeRelUpdate item);
    }
}
