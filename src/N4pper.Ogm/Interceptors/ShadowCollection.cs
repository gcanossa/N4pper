using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Entities;

namespace N4pper.Ogm.Interceptors
{
    public class ShadowCollection<T> : ShadowProperty, IList<T> where T : class, IOgmEntity
    {
        public ShadowCollection(GraphContextBase context, PropertyInfo property, IProxyTargetAccessor proxyAccessor) : base(context, property, proxyAccessor)
        {
        }

        public List<IOgmConnection> Connections { get; } = new List<IOgmConnection>();

        protected IOgmConnection GetConnection(IOgmEntity entity)
        {
            return entity is IOgmConnection ?
                entity as IOgmConnection
                :
                Connections.FirstOrDefault(p => IsInverse ? p.Destination == entity : p.Source == entity);
        }

        protected void DiscardEffect(IOgmEntity entity)
        {
            IOgmConnection conn = GetConnection(entity);
            if (conn != null)
                Context.ChangeTracker.Track(new EntityChangeRelDeletion(conn));
        }
        protected void DiscardEffects()
        {
            foreach (IOgmConnection item in Connections)
                Context.ChangeTracker.Track(new EntityChangeRelDeletion(item));
        }

        #region IList<T>

        public T this[int index]
        {
            get
            {
                return (IsInverse ? Connections[index].Source : Connections[index].Destination) as T;
            }
            set
            {
                IOgmEntity entity = value;
                if (entity is IOgmConnection && !IsInverse && (entity as IOgmConnection).Destination == null)
                    throw new InvalidOperationException($"'{nameof(IOgmConnection.Destination)}' property must be set.");
                if (entity is IOgmConnection && IsInverse && (entity as IOgmConnection).Source == null)
                    throw new InvalidOperationException($"'{nameof(IOgmConnection.Source)}' property must be set.");

                IOgmConnection tmp = Connections[index];
                Connections[index] = null;
                Unwire(tmp);

                long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                IOgmConnection conn = (entity as IOgmConnection) ??
                    new OgmConnection() { Order = Connections.Count, SourcePropertyName = SourceProperty?.Name ?? "", DestinationPropertyName = DestinationProperty?.Name ?? "", Version = version };

                Connections[index] = conn;

                if (conn is OgmConnection)
                {
                    conn.Source = IsInverse ? entity : ProxyAccessor as IOgmEntity;
                    conn.Destination = IsInverse ? ProxyAccessor as IOgmEntity : entity;
                }

                Wire(conn);

                for (int i = index; i < Connections.Count; i++)
                {
                    Connections[i].Order = i;
                }
            }
        }
        public int IndexOf(T item)
        {
            return Connections.IndexOf(GetConnection(item));
        }

        public void Insert(int index, T entity)
        {
            if (entity is IOgmConnection && !IsInverse && (entity as IOgmConnection).Destination == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Destination)}' property must be set.");
            if (entity is IOgmConnection && IsInverse && (entity as IOgmConnection).Source == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Source)}' property must be set.");

            long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            IOgmConnection conn = (entity as IOgmConnection) ??
                new OgmConnection() { Order = Connections.Count, SourcePropertyName = SourceProperty?.Name ?? "", DestinationPropertyName = DestinationProperty?.Name ?? "", Version = version };

            Connections.Insert(index, conn);

            if (conn is OgmConnection)
            {
                conn.Source = IsInverse ? entity : ProxyAccessor as IOgmEntity;
                conn.Destination = IsInverse ? ProxyAccessor as IOgmEntity : entity;
            }

            Wire(conn);

            for (int i = index; i < Connections.Count; i++)
            {
                Connections[i].Order = i;
            }
        }

        public void RemoveAt(int index)
        {
            Connections.RemoveAt(index);
        }

        #endregion

        #region ICollection<T>
        public int Count => Connections.Count;

        public bool IsReadOnly => false;
        
        public void Add(T entity)
        {
            if (entity is IOgmConnection && !IsInverse && (entity as IOgmConnection).Destination == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Destination)}' property must be set.");
            if (entity is IOgmConnection && IsInverse && (entity as IOgmConnection).Source == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Source)}' property must be set.");
            
            long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            IOgmConnection conn = (entity as IOgmConnection) ?? 
                new OgmConnection() { Order = Connections.Count, SourcePropertyName = SourceProperty?.Name ?? "", DestinationPropertyName = DestinationProperty?.Name ?? "", Version = version };

            Connections.Add(conn);

            if (conn is OgmConnection)
            {
                conn.Source = IsInverse ? entity : ProxyAccessor as IOgmEntity;
                conn.Destination = IsInverse ? ProxyAccessor as IOgmEntity : entity;
            }

            Wire(conn);
        }

        public void Clear()
        {
            if(IsInverse)
                foreach (IOgmConnection item in Connections)
                {
                    Remove(item.Source as T);
                }
            else
                foreach (IOgmConnection item in Connections)
                {
                    Remove(item.Destination as T);
                }
        }

        public bool Contains(T item)
        {
            return GetConnection(item) != null;
        }

        public bool Remove(T entity)
        {
            if (entity is IOgmConnection && !IsInverse && (entity as IOgmConnection).Destination == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Destination)}' property must be set.");
            if (entity is IOgmConnection && IsInverse && (entity as IOgmConnection).Source == null)
                throw new InvalidOperationException($"'{nameof(IOgmConnection.Source)}' property must be set.");

            DiscardEffect(entity);

            long version = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            IOgmConnection conn = GetConnection(entity);

            bool result = Connections.Remove(conn);

            Unwire(conn);
            
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (IsInverse)
                Connections.Select(p => p.Source as T).ToList().CopyTo(array, arrayIndex);
            else
                Connections.Select(p => p.Destination as T).ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if(IsInverse)
                return Connections.Select(p => p.Source as T).GetEnumerator();
            else
                return Connections.Select(p => p.Destination as T).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion
    }
}
