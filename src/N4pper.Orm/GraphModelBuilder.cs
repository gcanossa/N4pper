using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm
{
    public sealed class GraphModelBuilder
    {
        internal GraphModelBuilder()
        {

        }
        public GraphModelBuilder Entity<T>() where T : class
        {
            OrmCoreTypes.Entity<T>();
            return this;
        }
        public GraphModelBuilder Entity<T>(Expression<Func<T, object>> expr) where T : class
        {
            OrmCoreTypes.Entity<T>(expr);
            return this;
        }
        public GraphModelBuilder Entity<T>(IEnumerable<string> keyProps) where T : class
        {
            OrmCoreTypes.Entity<T>(keyProps);
            return this;
        }
    }
}
