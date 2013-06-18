namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal abstract class CommunicationPool<TKey, TItem> where TKey: class where TItem: class
    {
        private Dictionary<TKey, EndpointConnectionPool<TKey, TItem>> endpointPools;
        private int maxCount;
        private int openCount;
        private int pruneAccrual;
        private const int pruneThreshold = 30;

        protected CommunicationPool(int maxCount)
        {
            this.maxCount = maxCount;
            this.endpointPools = new Dictionary<TKey, EndpointConnectionPool<TKey, TItem>>();
            this.openCount = 1;
        }

        protected abstract void AbortItem(TItem item);
        public void AddConnection(TKey key, TItem connection, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.GetEndpointPool(key, helper.RemainingTime()).AddConnection(connection, helper.RemainingTime());
        }

        public bool Close(TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                if (this.openCount <= 0)
                {
                    return true;
                }
                this.openCount--;
                if (this.openCount == 0)
                {
                    this.OnClose(timeout);
                    return true;
                }
                return false;
            }
        }

        protected abstract void CloseItem(TItem item, TimeSpan timeout);
        protected abstract void CloseItemAsync(TItem item, TimeSpan timeout);
        protected virtual EndpointConnectionPool<TKey, TItem> CreateEndpointConnectionPool(TKey key)
        {
            return new EndpointConnectionPool<TKey, TItem>((CommunicationPool<TKey, TItem>) this, key);
        }

        private EndpointConnectionPool<TKey, TItem> GetEndpointPool(TKey key, TimeSpan timeout)
        {
            EndpointConnectionPool<TKey, TItem> pool = null;
            List<TItem> list = null;
            lock (this.ThisLock)
            {
                if (!this.endpointPools.TryGetValue(key, out pool))
                {
                    list = this.PruneIfNecessary();
                    pool = this.CreateEndpointConnectionPool(key);
                    this.endpointPools.Add(key, pool);
                }
            }
            if ((list != null) && (list.Count > 0))
            {
                TimeoutHelper helper = new TimeoutHelper(TimeoutHelper.Divide(timeout, 2));
                for (int i = 0; i < list.Count; i++)
                {
                    pool.CloseIdleConnection(list[i], helper.RemainingTime());
                }
            }
            return pool;
        }

        protected abstract TKey GetPoolKey(EndpointAddress address, Uri via);
        private void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            foreach (EndpointConnectionPool<TKey, TItem> pool in this.endpointPools.Values)
            {
                try
                {
                    pool.Close(helper.RemainingTime());
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x40002, System.ServiceModel.SR.GetString("TraceCodeConnectionPoolCloseException"), this, exception);
                    }
                }
                catch (TimeoutException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x40002, System.ServiceModel.SR.GetString("TraceCodeConnectionPoolCloseException"), this, exception2);
                    }
                }
            }
            this.endpointPools.Clear();
        }

        protected virtual void OnClosed()
        {
        }

        private List<TItem> PruneIfNecessary()
        {
            List<TItem> itemsToClose = null;
            this.pruneAccrual++;
            if (this.pruneAccrual > 30)
            {
                this.pruneAccrual = 0;
                itemsToClose = new List<TItem>();
                foreach (EndpointConnectionPool<TKey, TItem> pool in this.endpointPools.Values)
                {
                    pool.Prune(itemsToClose);
                }
                List<TKey> list2 = null;
                foreach (KeyValuePair<TKey, EndpointConnectionPool<TKey, TItem>> pair in this.endpointPools)
                {
                    if (pair.Value.CloseIfEmpty())
                    {
                        if (list2 == null)
                        {
                            list2 = new List<TKey>();
                        }
                        list2.Add(pair.Key);
                    }
                }
                if (list2 == null)
                {
                    return itemsToClose;
                }
                for (int i = 0; i < list2.Count; i++)
                {
                    this.endpointPools.Remove(list2[i]);
                }
            }
            return itemsToClose;
        }

        public void ReturnConnection(TKey key, TItem connection, bool connectionIsStillGood, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.GetEndpointPool(key, helper.RemainingTime()).ReturnConnection(connection, connectionIsStillGood, helper.RemainingTime());
        }

        public TItem TakeConnection(EndpointAddress address, Uri via, TimeSpan timeout, out TKey key)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            key = this.GetPoolKey(address, via);
            EndpointConnectionPool<TKey, TItem> endpointPool = this.GetEndpointPool(key, helper.RemainingTime());
            return endpointPool.TakeConnection(helper.RemainingTime());
        }

        public bool TryOpen()
        {
            lock (this.ThisLock)
            {
                if (this.openCount <= 0)
                {
                    return false;
                }
                this.openCount++;
                return true;
            }
        }

        public int MaxIdleConnectionPoolCount
        {
            get
            {
                return this.maxCount;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this;
            }
        }

        protected class EndpointConnectionPool
        {
            private List<TItem> busyConnections;
            private bool closed;
            private CommunicationPool<TKey, TItem>.IdleConnectionPool idleConnections;
            private TKey key;
            private CommunicationPool<TKey, TItem> parent;

            public EndpointConnectionPool(CommunicationPool<TKey, TItem> parent, TKey key)
            {
                this.key = key;
                this.parent = parent;
                this.busyConnections = new List<TItem>();
            }

            public void Abort()
            {
                if (!this.closed)
                {
                    List<TItem> idleItemsToClose = null;
                    lock (this.ThisLock)
                    {
                        if (this.closed)
                        {
                            return;
                        }
                        this.closed = true;
                        idleItemsToClose = this.SnapshotIdleConnections();
                    }
                    this.AbortConnections(idleItemsToClose);
                }
            }

            private void AbortConnections(List<TItem> idleItemsToClose)
            {
                for (int i = 0; i < idleItemsToClose.Count; i++)
                {
                    this.AbortItem(idleItemsToClose[i]);
                }
                for (int j = 0; j < this.busyConnections.Count; j++)
                {
                    this.AbortItem(this.busyConnections[j]);
                }
                this.busyConnections.Clear();
            }

            protected virtual void AbortItem(TItem item)
            {
                this.parent.AbortItem(item);
            }

            public void AddConnection(TItem connection, TimeSpan timeout)
            {
                bool flag = false;
                lock (this.ThisLock)
                {
                    if (!this.closed)
                    {
                        if (!this.IdleConnections.Add(connection))
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.CloseIdleConnection(connection, timeout);
                }
            }

            public void Close(TimeSpan timeout)
            {
                List<TItem> idleItemsToClose = null;
                lock (this.ThisLock)
                {
                    if (this.closed)
                    {
                        return;
                    }
                    this.closed = true;
                    idleItemsToClose = this.SnapshotIdleConnections();
                }
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    for (int i = 0; i < idleItemsToClose.Count; i++)
                    {
                        this.CloseItem(idleItemsToClose[i], helper.RemainingTime());
                    }
                    idleItemsToClose.Clear();
                }
                finally
                {
                    this.AbortConnections(idleItemsToClose);
                }
            }

            public void CloseIdleConnection(TItem connection, TimeSpan timeout)
            {
                bool flag = true;
                try
                {
                    this.CloseItemAsync(connection, timeout);
                    flag = false;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortItem(connection);
                    }
                }
            }

            public bool CloseIfEmpty()
            {
                lock (this.ThisLock)
                {
                    if (!this.closed)
                    {
                        if (this.busyConnections.Count > 0)
                        {
                            return false;
                        }
                        if ((this.idleConnections != null) && (this.idleConnections.Count > 0))
                        {
                            return false;
                        }
                        this.closed = true;
                    }
                }
                return true;
            }

            protected virtual void CloseItem(TItem item, TimeSpan timeout)
            {
                this.parent.CloseItem(item, timeout);
            }

            protected virtual void CloseItemAsync(TItem item, TimeSpan timeout)
            {
                this.parent.CloseItemAsync(item, timeout);
            }

            protected virtual CommunicationPool<TKey, TItem>.IdleConnectionPool GetIdleConnectionPool()
            {
                return new PoolIdleConnectionPool<TKey, TItem>(this.parent.MaxIdleConnectionPoolCount);
            }

            protected virtual void OnConnectionAborted()
            {
            }

            public virtual void Prune(List<TItem> itemsToClose)
            {
            }

            public void ReturnConnection(TItem connection, bool connectionIsStillGood, TimeSpan timeout)
            {
                bool flag = false;
                bool flag2 = false;
                lock (this.ThisLock)
                {
                    if (!this.closed)
                    {
                        if (this.busyConnections.Remove(connection) && connectionIsStillGood)
                        {
                            if (!this.IdleConnections.Return(connection))
                            {
                                flag = true;
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    else
                    {
                        flag2 = true;
                    }
                }
                if (flag)
                {
                    this.CloseIdleConnection(connection, timeout);
                }
                else if (flag2)
                {
                    this.AbortItem(connection);
                    this.OnConnectionAborted();
                }
            }

            private List<TItem> SnapshotIdleConnections()
            {
                List<TItem> list = new List<TItem>();
                while (true)
                {
                    bool flag;
                    TItem item = this.IdleConnections.Take(out flag);
                    if (item == null)
                    {
                        return list;
                    }
                    list.Add(item);
                }
            }

            public TItem TakeConnection(TimeSpan timeout)
            {
                TItem item = default(TItem);
                List<TItem> list = null;
                lock (this.ThisLock)
                {
                    if (this.closed)
                    {
                        return default(TItem);
                    }
                    while (true)
                    {
                        bool flag;
                        item = this.IdleConnections.Take(out flag);
                        if (item == null)
                        {
                            goto Label_007D;
                        }
                        if (!flag)
                        {
                            this.busyConnections.Add(item);
                            goto Label_007D;
                        }
                        if (list == null)
                        {
                            list = new List<TItem>();
                        }
                        list.Add(item);
                    }
                }
            Label_007D:
                if (list != null)
                {
                    TimeoutHelper helper = new TimeoutHelper(TimeoutHelper.Divide(timeout, 2));
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.CloseIdleConnection(list[i], helper.RemainingTime());
                    }
                }
                return item;
            }

            private CommunicationPool<TKey, TItem>.IdleConnectionPool IdleConnections
            {
                get
                {
                    if (this.idleConnections == null)
                    {
                        this.idleConnections = this.GetIdleConnectionPool();
                    }
                    return this.idleConnections;
                }
            }

            protected TKey Key
            {
                get
                {
                    return this.key;
                }
            }

            protected CommunicationPool<TKey, TItem> Parent
            {
                get
                {
                    return this.parent;
                }
            }

            protected object ThisLock
            {
                get
                {
                    return this;
                }
            }

            protected class PoolIdleConnectionPool : CommunicationPool<TKey, TItem>.IdleConnectionPool
            {
                private Pool<TItem> idleConnections;
                private int maxCount;

                public PoolIdleConnectionPool(int maxCount)
                {
                    this.idleConnections = new Pool<TItem>(maxCount);
                    this.maxCount = maxCount;
                }

                public override bool Add(TItem connection)
                {
                    return this.ReturnToPool(connection);
                }

                public override bool Return(TItem connection)
                {
                    return this.ReturnToPool(connection);
                }

                private bool ReturnToPool(TItem connection)
                {
                    bool flag = this.idleConnections.Return(connection);
                    if (!flag && DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40005, System.ServiceModel.SR.GetString("TraceCodeConnectionPoolMaxOutboundConnectionsPerEndpointQuotaReached", new object[] { this.maxCount }), this);
                    }
                    return flag;
                }

                public override TItem Take(out bool closeItem)
                {
                    closeItem = false;
                    return this.idleConnections.Take();
                }

                public override int Count
                {
                    get
                    {
                        return this.idleConnections.Count;
                    }
                }
            }
        }

        protected abstract class IdleConnectionPool
        {
            protected IdleConnectionPool()
            {
            }

            public abstract bool Add(TItem item);
            public abstract bool Return(TItem item);
            public abstract TItem Take(out bool closeItem);

            public abstract int Count { get; }
        }
    }
}

