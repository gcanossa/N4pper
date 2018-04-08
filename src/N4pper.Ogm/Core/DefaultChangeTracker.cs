using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Ogm.Core
{
    public class DefaultChangeTracker : ChangeTrackerBase
    {
        protected override void NodeCreationChange(EntityChangeNodeCreation item)
        {
            IEnumerable<EntityChangeDescriptor> temp = ChangeLog.Where(p=>p.Entity == item.Entity);
            if (temp.Any(p => p is EntityChangeNodeCreation == false))
                throw new InvalidOperationException("Unable to schedule a node creation for an entity already tracked.");

            if (!temp.Any())
                ChangeLog.Add(item);
        }

        protected override void NodeDeletionChange(EntityChangeNodeDeletion item)
        {
            IEnumerable<EntityChangeDescriptor> temp = ChangeLog.Where(p => p.Entity == item.Entity).ToList();

            if (!temp.Any() && item.Entity.EntityId == null)
                throw new InvalidOperationException("Unable to locate entity. It must be tracked or have an EntityId.");
            
            if(temp.Any(p=>p is EntityChangeNodeCreation))
            {
                ChangeLog.RemoveAll(p => temp.Contains(p));
            }
            else
            {
                ChangeLog.RemoveAll(p => temp.Contains(p));
                ChangeLog.Add(item);
            }

            foreach (EntityChangeRelCreation rel in ChangeLog.Where(p => p is EntityChangeRelCreation && (((EntityChangeRelCreation)p).Source == item.Entity || ((EntityChangeRelCreation)p).Destination == item.Entity)).ToList())
            {
                RelDeletionChange(new EntityChangeRelDeletion(rel.Entity as Connection));
            }
        }

        protected override void NodeUpdateChange(EntityChangeNodeUpdate item)
        {
            IEnumerable<EntityChangeDescriptor> temp = ChangeLog.Where(p => p.Entity == item.Entity).ToList();

            if (!temp.Any() && item.Entity.EntityId == null)
                throw new InvalidOperationException("Unable to locate entity. It must be tracked or have an EntityId.");

            if (temp.Any(p => p is EntityChangeNodeDeletion))
                throw new InvalidOperationException("Unable to update an entity scheduled for deletion");

            if (temp.Any(p => p is EntityChangeNodeCreation))
                return;

            if (temp.Any(p => p.Inverse != null && p.Inverse.Equals(item)))
                ChangeLog.Remove(item);
            else
            {
                if (temp.Any(p => (p as EntityChangeNodeUpdate)?.Property == item.Property))
                {
                    ChangeLog.RemoveAll(p => temp.Contains(p) && (p as EntityChangeNodeUpdate)?.Property == item.Property);
                }
                ChangeLog.Add(item);
            }
        }

        protected override void RelCreationChange(EntityChangeRelCreation item)
        {
            IEnumerable<EntityChangeDescriptor> temp = ChangeLog.Where(p => p.Entity == item.Entity);
            if (temp.Any(p => p is EntityChangeRelCreation == false))
                throw new InvalidOperationException("Unable to schedule a relationship creation for an entity already tracked.");
            if (temp.Any() && temp.Any(p => !p.Equals(item)))
                throw new InvalidOperationException("Unable to schedule a relationship creation. Already tracked for another source-destination pair.");

            if (ChangeLog.Any(p => p is EntityChangeNodeDeletion && p.Entity == item.Source) || 
                item.Source.EntityId == null && !ChangeLog.Any(p =>  p is EntityChangeNodeCreation && p.Entity == item.Source))
                throw new ArgumentException("Invalid source node");
            if (ChangeLog.Any(p => p is EntityChangeNodeDeletion && p.Entity == item.Destination) || 
                item.Destination.EntityId == null && !ChangeLog.Any(p => p is EntityChangeNodeCreation && p.Entity == item.Destination))
                throw new ArgumentException("Invalid destination node");

            if (!temp.Any())
                ChangeLog.Add(item);
        }

        protected override void RelDeletionChange(EntityChangeRelDeletion item)
        {
            IEnumerable<EntityChangeDescriptor> temp = ChangeLog.Where(p => p.Entity == item.Entity).ToList();

            if (!temp.Any() && item.Entity.EntityId == null)
                throw new InvalidOperationException("Unable to locate entity. It must be tracked or have an EntityId.");

            if (temp.Any(p => p is EntityChangeRelCreation))
            {
                ChangeLog.RemoveAll(p => temp.Contains(p));
            }
            else
            {
                ChangeLog.RemoveAll(p => temp.Contains(p));
                ChangeLog.Add(item);
            }
        }

        protected override void RelUpdateChange(EntityChangeRelUpdate item)
        {
            IEnumerable<EntityChangeDescriptor> temp = ChangeLog.Where(p => p.Entity == item.Entity).ToList();

            if (!temp.Any() && item.Entity.EntityId == null)
                throw new InvalidOperationException("Unable to locate entity. It must be tracked or have an EntityId.");

            if (temp.Any(p => p is EntityChangeRelDeletion))
                throw new InvalidOperationException("Unable to update an entity scheduled for deletion");

            if (temp.Any(p => p is EntityChangeRelCreation))
                return;

            if (temp.Any(p => p.Inverse != null && p.Inverse.Equals(item)))
                ChangeLog.Remove(item);
            else
            {
                if (temp.Any(p => (p as EntityChangeRelUpdate)?.Property == item.Property))
                {
                    ChangeLog.RemoveAll(p => temp.Contains(p) && (p as EntityChangeRelUpdate)?.Property == item.Property);
                }
                ChangeLog.Add(item);
            }
        }
    }
}
