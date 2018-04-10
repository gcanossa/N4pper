using N4pper.Ogm.Entities;
using N4pper.Ogm.Queryable;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Ogm
{
    public interface IGraphContext : IDisposable
    {
        IStatementRunner Runner { get; }
        T Add<T>(T obj) where T : IOgmEntity;
        void Remove(IOgmEntity obj);
        T Attach<T>(T obj) where T : IOgmEntity;
        T Detach<T>(T obj) where T : IOgmEntity;
        void SaveChanges();
        IQueryable<T> Query<T>(Action<IInclude<T>> includes = null) where T : class, IOgmEntity;
    }
}
