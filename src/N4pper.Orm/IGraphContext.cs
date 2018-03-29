using N4pper.Orm.Queryable;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm
{
    public interface IGraphContext : IDisposable
    {
        IStatementRunner Runner { get; }
        void Add(object obj);
        void Remove(object obj);
        void Detach(object obj);
        void SaveChanges();
        IQueryable<T> Query<T>(Action<IInclude<T>> includes = null) where T : class;
    }
}
