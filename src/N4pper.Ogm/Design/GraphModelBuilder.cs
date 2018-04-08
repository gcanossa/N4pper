using N4pper.Ogm.Design;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Ogm.Design
{
    public sealed class GraphModelBuilder
    {
        internal GraphModelBuilder()
        {

        }
        public IConstraintBuilder<T> Entity<T>() where T : class, Entities.IOgmEntity
        {
            if (typeof(Entities.ExplicitConnection).IsAssignableFrom(typeof(T)))
                throw new ArgumentException($"To register an explicit connetion type use '{nameof(ConnectionEntity)}'.");

            TypesManager.Entity<T>();
            return new GenericBuilder<T>();
        }
        public IConstraintBuilder<T> ConnectionEntity<T>() where T : Entities.ExplicitConnection
        {
            TypesManager.Entity<T>();
            return new GenericBuilder<T>();
        }
    }
}
