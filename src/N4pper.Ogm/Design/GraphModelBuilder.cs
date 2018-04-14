using N4pper.Ogm.Design;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Ogm.Design
{
    public sealed class GraphModelBuilder
    {
        public TypesManager Manager { get; set; }
        public GraphModelBuilder(TypesManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }
        public IConstraintBuilder<T> Entity<T>(bool ignoreUsupported = false) where T : class, Entities.IOgmEntity
        {
            if (typeof(Entities.IOgmConnection).IsAssignableFrom(typeof(T)))
                throw new ArgumentException($"To register an explicit connetion type use '{nameof(ConnectionEntity)}'.");

            Manager.Entity<T>(ignoreUsupported);
            return new GenericBuilder<T>(Manager);
        }
        public IConstraintBuilder<T> ConnectionEntity<T>(bool ignoreUsupported = false) where T : class, Entities.IOgmConnection
        {
            if (!typeof(Entities.IOgmConnection).IsAssignableFrom(typeof(T)))
                throw new ArgumentException($"To register a node type use '{nameof(Entity)}'.");

            Manager.Entity<T>(ignoreUsupported);
            return new GenericBuilder<T>(Manager);
        }
    }
}
