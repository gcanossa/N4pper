using N4pper.Orm.Design;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    public sealed class GraphModelBuilder
    {
        internal GraphModelBuilder()
        {

        }
        public IConstraintBuilder<T> Entity<T>() where T : class
        {
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(typeof(T)))
                throw new ArgumentException($"To register an explicit connetion type use '{nameof(ConnectionEntity)}'.");

            OrmCoreTypes.Entity<T>();
            return new GenericBuilder<T>();
        }
        public IConstraintBuilder<T> Entity<T>(Expression<Func<T, object>> expr) where T : class
        {
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(typeof(T)))
                throw new ArgumentException($"To register an explicit connetion type use '{nameof(ConnectionEntity)}'.");

            OrmCoreTypes.Entity<T>(expr);
            return new GenericBuilder<T>();
        }
        public IConstraintBuilder<T> Entity<T>(IEnumerable<string> keyProps) where T : class
        {
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(typeof(T)))
                throw new ArgumentException($"To register an explicit connetion type use '{nameof(ConnectionEntity)}'.");

            OrmCoreTypes.Entity<T>(keyProps);
            return new GenericBuilder<T>();
        }
        public IConstraintBuilder<T> ConnectionEntity<T>() where T : Entities.ExplicitConnection
        {
            OrmCoreTypes.Entity<T>();
            return new GenericBuilder<T>();
        }
    }
}
