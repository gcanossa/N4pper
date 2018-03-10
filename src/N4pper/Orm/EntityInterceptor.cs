using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm
{
    public abstract class EntityInterceptor<T> where T : class
    {
        public virtual void OnQueryBuilding(StatementBuilder builder, Dictionary<string, object> obj) { }
        public virtual void OnAddOrUpdateBuilding(StatementBuilder builder, Dictionary<string, object> obj) { }
        public virtual void OnDeleteBuilding(StatementBuilder builder, Dictionary<string, object> obj) { }

        public virtual void OnQuery(Dictionary<string, object> obj) { }
        public virtual void OnAddOrUpdate(Dictionary<string, object> obj) { }
        public virtual void OnDelete(Dictionary<string, object> obj) { }
    }
}
