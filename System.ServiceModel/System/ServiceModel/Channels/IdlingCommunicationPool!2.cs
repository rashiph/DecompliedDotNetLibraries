namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal abstract class IdlingCommunicationPool<TKey, TItem> : CommunicationPool<TKey, TItem> where TKey: class where TItem: class
    {
        private TimeSpan idleTimeout;
        private TimeSpan leaseTimeout;

        protected IdlingCommunicationPool(int maxCount, TimeSpan idleTimeout, TimeSpan leaseTimeout) : base(maxCount)
        {
            this.idleTimeout = idleTimeout;
            this.leaseTimeout = leaseTimeout;
        }

        protected override void CloseItemAsync(TItem item, TimeSpan timeout)
        {
            this.CloseItem(item, timeout);
        }

        protected override CommunicationPool<TKey, TItem>.EndpointConnectionPool CreateEndpointConnectionPool(TKey key)
        {
            if (!(this.idleTimeout != TimeSpan.MaxValue) && !(this.leaseTimeout != TimeSpan.MaxValue))
            {
                return base.CreateEndpointConnectionPool(key);
            }
            return new IdleTimeoutEndpointConnectionPool<TKey, TItem>((IdlingCommunicationPool<TKey, TItem>) this, key);
        }

        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
        }

        protected TimeSpan LeaseTimeout
        {
            get
            {
                return this.leaseTimeout;
            }
        }

        protected class IdleTimeoutEndpointConnectionPool : CommunicationPool<TKey, TItem>.EndpointConnectionPool
        {
            private IdleTimeoutIdleConnectionPool<TKey, TItem> connections;

            public IdleTimeoutEndpointConnectionPool(IdlingCommunicationPool<TKey, TItem> parent, TKey key) : base(parent, key)
            {
                this.connections = new IdleTimeoutIdleConnectionPool<TKey, TItem>((IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool) this, base.ThisLock);
            }

            protected override void AbortItem(TItem item)
            {
                this.connections.OnItemClosing(item);
                base.AbortItem(item);
            }

            protected override void CloseItem(TItem item, TimeSpan timeout)
            {
                this.connections.OnItemClosing(item);
                base.CloseItem(item, timeout);
            }

            protected override void CloseItemAsync(TItem item, TimeSpan timeout)
            {
                this.connections.OnItemClosing(item);
                base.CloseItemAsync(item, timeout);
            }

            protected override CommunicationPool<TKey, TItem>.IdleConnectionPool GetIdleConnectionPool()
            {
                return this.connections;
            }

            public override void Prune(List<TItem> itemsToClose)
            {
                if (this.connections != null)
                {
                    this.connections.Prune(itemsToClose, false);
                }
            }

            protected class IdleTimeoutIdleConnectionPool : CommunicationPool<TKey, TItem>.EndpointConnectionPool.PoolIdleConnectionPool
            {
                private Dictionary<TItem, IdlingConnectionSettings<TKey, TItem>> connectionMapping;
                private TimeSpan idleTimeout;
                private IOThreadTimer idleTimer;
                private TimeSpan leaseTimeout;
                private static Action<object> onIdle;
                private IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool parent;
                private Exception pendingException;
                private object thisLock;
                private const int timerThreshold = 1;

                public IdleTimeoutIdleConnectionPool(IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool parent, object thisLock) : base(parent.Parent.MaxIdleConnectionPoolCount)
                {
                    this.parent = parent;
                    IdlingCommunicationPool<TKey, TItem> pool = (IdlingCommunicationPool<TKey, TItem>) parent.Parent;
                    this.idleTimeout = pool.idleTimeout;
                    this.leaseTimeout = pool.leaseTimeout;
                    this.thisLock = thisLock;
                    this.connectionMapping = new Dictionary<TItem, IdlingConnectionSettings<TKey, TItem>>();
                }

                public override bool Add(TItem connection)
                {
                    this.ThrowPendingException();
                    bool flag = base.Add(connection);
                    if (flag)
                    {
                        this.connectionMapping.Add(connection, new IdlingConnectionSettings<TKey, TItem>());
                        this.StartTimerIfNecessary();
                    }
                    return flag;
                }

                private void CancelTimer()
                {
                    if (this.idleTimer != null)
                    {
                        this.idleTimer.Cancel();
                    }
                }

                private bool IdleOutConnection(TItem connection, DateTime now)
                {
                    if (connection == null)
                    {
                        return false;
                    }
                    bool flag = false;
                    IdlingConnectionSettings<TKey, TItem> settings = this.connectionMapping[connection];
                    if (now > (settings.LastUsage + this.idleTimeout))
                    {
                        this.TraceConnectionIdleTimeoutExpired();
                        return true;
                    }
                    if ((now - settings.CreationTime) >= this.leaseTimeout)
                    {
                        this.TraceConnectionLeaseTimeoutExpired();
                        flag = true;
                    }
                    return flag;
                }

                private void OnIdle()
                {
                    List<TItem> itemsToClose = new List<TItem>();
                    lock (this.thisLock)
                    {
                        try
                        {
                            this.Prune(itemsToClose, true);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            this.pendingException = exception;
                            this.CancelTimer();
                        }
                    }
                    TimeoutHelper helper = new TimeoutHelper(TimeoutHelper.Divide(this.idleTimeout, 2));
                    for (int i = 0; i < itemsToClose.Count; i++)
                    {
                        this.parent.CloseIdleConnection(itemsToClose[i], helper.RemainingTime());
                    }
                }

                private static void OnIdle(object state)
                {
                    ((IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool.IdleTimeoutIdleConnectionPool) state).OnIdle();
                }

                public void OnItemClosing(TItem connection)
                {
                    this.ThrowPendingException();
                    lock (this.thisLock)
                    {
                        this.connectionMapping.Remove(connection);
                    }
                }

                public void Prune(List<TItem> itemsToClose, bool calledFromTimer)
                {
                    if (!calledFromTimer)
                    {
                        this.ThrowPendingException();
                    }
                    if (this.Count != 0)
                    {
                        DateTime utcNow = DateTime.UtcNow;
                        bool flag = false;
                        lock (this.thisLock)
                        {
                            TItem[] localArray = new TItem[this.Count];
                            for (int i = 0; i < localArray.Length; i++)
                            {
                                bool flag2;
                                localArray[i] = base.Take(out flag2);
                                if (flag2 || this.IdleOutConnection(localArray[i], utcNow))
                                {
                                    itemsToClose.Add(localArray[i]);
                                    localArray[i] = default(TItem);
                                }
                            }
                            for (int j = 0; j < localArray.Length; j++)
                            {
                                if (localArray[j] != null)
                                {
                                    base.Return(localArray[j]);
                                }
                            }
                            flag = this.Count > 0;
                        }
                        if (calledFromTimer && flag)
                        {
                            this.idleTimer.Set(this.idleTimeout);
                        }
                    }
                }

                public override bool Return(TItem connection)
                {
                    this.ThrowPendingException();
                    if (!this.connectionMapping.ContainsKey(connection))
                    {
                        return false;
                    }
                    bool flag = base.Return(connection);
                    if (flag)
                    {
                        this.connectionMapping[connection].LastUsage = DateTime.UtcNow;
                        this.StartTimerIfNecessary();
                    }
                    return flag;
                }

                private void StartTimerIfNecessary()
                {
                    if (this.Count > 1)
                    {
                        if (this.idleTimer == null)
                        {
                            if (IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool.IdleTimeoutIdleConnectionPool.onIdle == null)
                            {
                                IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool.IdleTimeoutIdleConnectionPool.onIdle = new Action<object>(IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool.IdleTimeoutIdleConnectionPool.OnIdle);
                            }
                            this.idleTimer = new IOThreadTimer(IdlingCommunicationPool<TKey, TItem>.IdleTimeoutEndpointConnectionPool.IdleTimeoutIdleConnectionPool.onIdle, this, false);
                        }
                        this.idleTimer.Set(this.idleTimeout);
                    }
                }

                public override TItem Take(out bool closeItem)
                {
                    this.ThrowPendingException();
                    DateTime utcNow = DateTime.UtcNow;
                    TItem connection = base.Take(out closeItem);
                    if (!closeItem)
                    {
                        closeItem = this.IdleOutConnection(connection, utcNow);
                    }
                    return connection;
                }

                private void ThrowPendingException()
                {
                    if (this.pendingException != null)
                    {
                        lock (this.thisLock)
                        {
                            if (this.pendingException != null)
                            {
                                Exception pendingException = this.pendingException;
                                this.pendingException = null;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(pendingException);
                            }
                        }
                    }
                }

                private void TraceConnectionIdleTimeoutExpired()
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40003, System.ServiceModel.SR.GetString("TraceCodeConnectionPoolIdleTimeoutReached", new object[] { this.idleTimeout }), this);
                    }
                }

                private void TraceConnectionLeaseTimeoutExpired()
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40004, System.ServiceModel.SR.GetString("TraceCodeConnectionPoolLeaseTimeoutReached", new object[] { this.leaseTimeout }), this);
                    }
                }

                private class IdlingConnectionSettings
                {
                    private DateTime creationTime;
                    private DateTime lastUsage;

                    public IdlingConnectionSettings()
                    {
                        this.creationTime = DateTime.UtcNow;
                        this.lastUsage = this.creationTime;
                    }

                    public DateTime CreationTime
                    {
                        get
                        {
                            return this.creationTime;
                        }
                    }

                    public DateTime LastUsage
                    {
                        get
                        {
                            return this.lastUsage;
                        }
                        set
                        {
                            this.lastUsage = value;
                        }
                    }
                }
            }
        }
    }
}

