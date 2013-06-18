namespace System.Data.ProviderBase
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class DbConnectionFactory
    {
        private Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup> _connectionPoolGroups;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        private readonly System.Data.ProviderBase.DbConnectionPoolCounters _performanceCounters;
        private readonly List<System.Data.ProviderBase.DbConnectionPoolGroup> _poolGroupsToRelease;
        private readonly List<System.Data.ProviderBase.DbConnectionPool> _poolsToRelease;
        private readonly Timer _pruningTimer;
        private const int PruningDueTime = 0x3a980;
        private const int PruningPeriod = 0x7530;

        protected DbConnectionFactory() : this(System.Data.ProviderBase.DbConnectionPoolCountersNoCounters.SingletonInstance)
        {
        }

        protected DbConnectionFactory(System.Data.ProviderBase.DbConnectionPoolCounters performanceCounters)
        {
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this._performanceCounters = performanceCounters;
            this._connectionPoolGroups = new Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup>();
            this._poolsToRelease = new List<System.Data.ProviderBase.DbConnectionPool>();
            this._poolGroupsToRelease = new List<System.Data.ProviderBase.DbConnectionPoolGroup>();
            this._pruningTimer = this.CreatePruningTimer();
        }

        public void ClearAllPools()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<prov.DbConnectionFactory.ClearAllPools|API> ");
            try
            {
                foreach (KeyValuePair<string, System.Data.ProviderBase.DbConnectionPoolGroup> pair in this._connectionPoolGroups)
                {
                    System.Data.ProviderBase.DbConnectionPoolGroup group = pair.Value;
                    if (group != null)
                    {
                        group.Clear();
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void ClearPool(DbConnection connection)
        {
            IntPtr ptr;
            System.Data.Common.ADP.CheckArgumentNull(connection, "connection");
            Bid.ScopeEnter(out ptr, "<prov.DbConnectionFactory.ClearPool|API> %d#", this.GetObjectId(connection));
            try
            {
                System.Data.ProviderBase.DbConnectionPoolGroup connectionPoolGroup = this.GetConnectionPoolGroup(connection);
                if (connectionPoolGroup != null)
                {
                    connectionPoolGroup.Clear();
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        protected abstract System.Data.ProviderBase.DbConnectionInternal CreateConnection(System.Data.Common.DbConnectionOptions options, object poolGroupProviderInfo, System.Data.ProviderBase.DbConnectionPool pool, DbConnection owningConnection);
        protected abstract System.Data.Common.DbConnectionOptions CreateConnectionOptions(string connectionString, System.Data.Common.DbConnectionOptions previous);
        protected abstract System.Data.ProviderBase.DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(System.Data.Common.DbConnectionOptions options);
        internal virtual System.Data.ProviderBase.DbConnectionPoolGroupProviderInfo CreateConnectionPoolGroupProviderInfo(System.Data.Common.DbConnectionOptions connectionOptions)
        {
            return null;
        }

        internal virtual System.Data.ProviderBase.DbConnectionPoolProviderInfo CreateConnectionPoolProviderInfo(System.Data.Common.DbConnectionOptions connectionOptions)
        {
            return null;
        }

        protected virtual System.Data.ProviderBase.DbMetaDataFactory CreateMetaDataFactory(System.Data.ProviderBase.DbConnectionInternal internalConnection, out bool cacheMetaDataFactory)
        {
            cacheMetaDataFactory = false;
            throw System.Data.Common.ADP.NotSupported();
        }

        internal System.Data.ProviderBase.DbConnectionInternal CreateNonPooledConnection(DbConnection owningConnection, System.Data.ProviderBase.DbConnectionPoolGroup poolGroup)
        {
            System.Data.Common.DbConnectionOptions connectionOptions = poolGroup.ConnectionOptions;
            System.Data.ProviderBase.DbConnectionPoolGroupProviderInfo providerInfo = poolGroup.ProviderInfo;
            System.Data.ProviderBase.DbConnectionInternal internal2 = this.CreateConnection(connectionOptions, providerInfo, null, owningConnection);
            if (internal2 != null)
            {
                this.PerformanceCounters.HardConnectsPerSecond.Increment();
                internal2.MakeNonPooledObject(owningConnection, this.PerformanceCounters);
            }
            Bid.Trace("<prov.DbConnectionFactory.CreateNonPooledConnection|RES|CPOOL> %d#, Non-pooled database connection created.\n", this.ObjectID);
            return internal2;
        }

        internal System.Data.ProviderBase.DbConnectionInternal CreatePooledConnection(DbConnection owningConnection, System.Data.ProviderBase.DbConnectionPool pool, System.Data.Common.DbConnectionOptions options)
        {
            System.Data.ProviderBase.DbConnectionPoolGroupProviderInfo providerInfo = pool.PoolGroup.ProviderInfo;
            System.Data.ProviderBase.DbConnectionInternal internal2 = this.CreateConnection(options, providerInfo, pool, owningConnection);
            if (internal2 != null)
            {
                this.PerformanceCounters.HardConnectsPerSecond.Increment();
                internal2.MakePooledConnection(pool);
            }
            Bid.Trace("<prov.DbConnectionFactory.CreatePooledConnection|RES|CPOOL> %d#, Pooled database connection created.\n", this.ObjectID);
            return internal2;
        }

        private Timer CreatePruningTimer()
        {
            return new Timer(new TimerCallback(this.PruneConnectionPoolGroups), null, 0x3a980, 0x7530);
        }

        internal System.Data.ProviderBase.DbConnectionInternal GetConnection(DbConnection owningConnection)
        {
            System.Data.ProviderBase.DbConnectionInternal connection;
            int num2 = 10;
            int millisecondsTimeout = 1;
            do
            {
                System.Data.ProviderBase.DbConnectionPoolGroup connectionPoolGroup = this.GetConnectionPoolGroup(owningConnection);
                System.Data.ProviderBase.DbConnectionPool connectionPool = this.GetConnectionPool(owningConnection, connectionPoolGroup);
                if (connectionPool == null)
                {
                    connectionPoolGroup = this.GetConnectionPoolGroup(owningConnection);
                    connection = this.CreateNonPooledConnection(owningConnection, connectionPoolGroup);
                    this.PerformanceCounters.NumberOfNonPooledConnections.Increment();
                }
                else
                {
                    connection = connectionPool.GetConnection(owningConnection);
                    if (connection == null)
                    {
                        if (connectionPool.IsRunning)
                        {
                            Bid.Trace("<prov.DbConnectionFactory.GetConnection|RES|CPOOL> %d#, GetConnection failed because a pool timeout occurred.\n", this.ObjectID);
                            throw System.Data.Common.ADP.PooledOpenTimeout();
                        }
                        Thread.Sleep(millisecondsTimeout);
                        millisecondsTimeout *= 2;
                    }
                }
            }
            while ((connection == null) && (num2-- > 0));
            if (connection == null)
            {
                Bid.Trace("<prov.DbConnectionFactory.GetConnection|RES|CPOOL> %d#, GetConnection failed because a pool timeout occurred and all retries were exhausted.\n", this.ObjectID);
                throw System.Data.Common.ADP.PooledOpenTimeout();
            }
            return connection;
        }

        private System.Data.ProviderBase.DbConnectionPool GetConnectionPool(DbConnection owningObject, System.Data.ProviderBase.DbConnectionPoolGroup connectionPoolGroup)
        {
            if (connectionPoolGroup.IsDisabled && (connectionPoolGroup.PoolGroupOptions != null))
            {
                Bid.Trace("<prov.DbConnectionFactory.GetConnectionPool|RES|INFO|CPOOL> %d#, DisabledPoolGroup=%d#\n", this.ObjectID, connectionPoolGroup.ObjectID);
                System.Data.ProviderBase.DbConnectionPoolGroupOptions poolGroupOptions = connectionPoolGroup.PoolGroupOptions;
                System.Data.Common.DbConnectionOptions connectionOptions = connectionPoolGroup.ConnectionOptions;
                string connectionString = connectionOptions.UsersConnectionString(false);
                connectionPoolGroup = this.GetConnectionPoolGroup(connectionString, poolGroupOptions, ref connectionOptions);
                this.SetConnectionPoolGroup(owningObject, connectionPoolGroup);
            }
            return connectionPoolGroup.GetConnectionPool(this);
        }

        internal abstract System.Data.ProviderBase.DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection);
        internal System.Data.ProviderBase.DbConnectionPoolGroup GetConnectionPoolGroup(string connectionString, System.Data.ProviderBase.DbConnectionPoolGroupOptions poolOptions, ref System.Data.Common.DbConnectionOptions userConnectionOptions)
        {
            System.Data.ProviderBase.DbConnectionPoolGroup group;
            if (System.Data.Common.ADP.IsEmpty(connectionString))
            {
                return null;
            }
            if (!this._connectionPoolGroups.TryGetValue(connectionString, out group) || (group.IsDisabled && (group.PoolGroupOptions != null)))
            {
                System.Data.Common.DbConnectionOptions options = this.CreateConnectionOptions(connectionString, userConnectionOptions);
                if (options == null)
                {
                    throw System.Data.Common.ADP.InternalConnectionError(System.Data.Common.ADP.ConnectionError.ConnectionOptionsMissing);
                }
                string str = connectionString;
                if (userConnectionOptions == null)
                {
                    userConnectionOptions = options;
                    str = options.Expand();
                    if (str != connectionString)
                    {
                        return this.GetConnectionPoolGroup(str, null, ref userConnectionOptions);
                    }
                }
                if ((poolOptions == null) && System.Data.Common.ADP.IsWindowsNT)
                {
                    if (group != null)
                    {
                        poolOptions = group.PoolGroupOptions;
                    }
                    else
                    {
                        poolOptions = this.CreateConnectionPoolGroupOptions(options);
                    }
                }
                System.Data.ProviderBase.DbConnectionPoolGroup group2 = new System.Data.ProviderBase.DbConnectionPoolGroup(options, poolOptions) {
                    ProviderInfo = this.CreateConnectionPoolGroupProviderInfo(options)
                };
                lock (this)
                {
                    Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup> dictionary = this._connectionPoolGroups;
                    if (!dictionary.TryGetValue(str, out group))
                    {
                        Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup> dictionary2 = new Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup>(1 + dictionary.Count);
                        foreach (KeyValuePair<string, System.Data.ProviderBase.DbConnectionPoolGroup> pair in dictionary)
                        {
                            dictionary2.Add(pair.Key, pair.Value);
                        }
                        dictionary2.Add(str, group2);
                        this.PerformanceCounters.NumberOfActiveConnectionPoolGroups.Increment();
                        group = group2;
                        this._connectionPoolGroups = dictionary2;
                    }
                    return group;
                }
            }
            if (userConnectionOptions == null)
            {
                userConnectionOptions = group.ConnectionOptions;
            }
            return group;
        }

        internal abstract System.Data.ProviderBase.DbConnectionInternal GetInnerConnection(DbConnection connection);
        internal System.Data.ProviderBase.DbMetaDataFactory GetMetaDataFactory(System.Data.ProviderBase.DbConnectionPoolGroup connectionPoolGroup, System.Data.ProviderBase.DbConnectionInternal internalConnection)
        {
            System.Data.ProviderBase.DbMetaDataFactory metaDataFactory = connectionPoolGroup.MetaDataFactory;
            if (metaDataFactory == null)
            {
                bool cacheMetaDataFactory = false;
                metaDataFactory = this.CreateMetaDataFactory(internalConnection, out cacheMetaDataFactory);
                if (cacheMetaDataFactory)
                {
                    connectionPoolGroup.MetaDataFactory = metaDataFactory;
                }
            }
            return metaDataFactory;
        }

        protected abstract int GetObjectId(DbConnection connection);
        internal abstract void PermissionDemand(DbConnection outerConnection);
        private void PruneConnectionPoolGroups(object state)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<prov.DbConnectionFactory.PruneConnectionPoolGroups|RES|INFO|CPOOL> %d#\n", this.ObjectID);
            }
            lock (this._poolsToRelease)
            {
                if (this._poolsToRelease.Count != 0)
                {
                    foreach (System.Data.ProviderBase.DbConnectionPool pool in this._poolsToRelease.ToArray())
                    {
                        if (pool != null)
                        {
                            pool.Clear();
                            if (pool.Count == 0)
                            {
                                this._poolsToRelease.Remove(pool);
                                if (Bid.AdvancedOn)
                                {
                                    Bid.Trace("<prov.DbConnectionFactory.PruneConnectionPoolGroups|RES|INFO|CPOOL> %d#, ReleasePool=%d#\n", this.ObjectID, pool.ObjectID);
                                }
                                this.PerformanceCounters.NumberOfInactiveConnectionPools.Decrement();
                            }
                        }
                    }
                }
            }
            lock (this._poolGroupsToRelease)
            {
                if (this._poolGroupsToRelease.Count != 0)
                {
                    foreach (System.Data.ProviderBase.DbConnectionPoolGroup group in this._poolGroupsToRelease.ToArray())
                    {
                        if (group != null)
                        {
                            group.Clear();
                            if (group.Count == 0)
                            {
                                this._poolGroupsToRelease.Remove(group);
                                if (Bid.AdvancedOn)
                                {
                                    Bid.Trace("<prov.DbConnectionFactory.PruneConnectionPoolGroups|RES|INFO|CPOOL> %d#, ReleasePoolGroup=%d#\n", this.ObjectID, group.ObjectID);
                                }
                                this.PerformanceCounters.NumberOfInactiveConnectionPoolGroups.Decrement();
                            }
                        }
                    }
                }
            }
            lock (this)
            {
                Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup> dictionary2 = this._connectionPoolGroups;
                Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup> dictionary = new Dictionary<string, System.Data.ProviderBase.DbConnectionPoolGroup>(dictionary2.Count);
                foreach (KeyValuePair<string, System.Data.ProviderBase.DbConnectionPoolGroup> pair in dictionary2)
                {
                    if (pair.Value != null)
                    {
                        if (pair.Value.Prune())
                        {
                            this.PerformanceCounters.NumberOfActiveConnectionPoolGroups.Decrement();
                            this.QueuePoolGroupForRelease(pair.Value);
                        }
                        else
                        {
                            dictionary.Add(pair.Key, pair.Value);
                        }
                    }
                }
                this._connectionPoolGroups = dictionary;
            }
        }

        internal void QueuePoolForRelease(System.Data.ProviderBase.DbConnectionPool pool, bool clearing)
        {
            pool.Shutdown();
            lock (this._poolsToRelease)
            {
                if (clearing)
                {
                    pool.Clear();
                }
                this._poolsToRelease.Add(pool);
            }
            this.PerformanceCounters.NumberOfInactiveConnectionPools.Increment();
        }

        internal void QueuePoolGroupForRelease(System.Data.ProviderBase.DbConnectionPoolGroup poolGroup)
        {
            Bid.Trace("<prov.DbConnectionFactory.QueuePoolGroupForRelease|RES|INFO|CPOOL> %d#, poolGroup=%d#\n", this.ObjectID, poolGroup.ObjectID);
            lock (this._poolGroupsToRelease)
            {
                this._poolGroupsToRelease.Add(poolGroup);
            }
            this.PerformanceCounters.NumberOfInactiveConnectionPoolGroups.Increment();
        }

        internal abstract void SetConnectionPoolGroup(DbConnection outerConnection, System.Data.ProviderBase.DbConnectionPoolGroup poolGroup);
        internal abstract void SetInnerConnectionEvent(DbConnection owningObject, System.Data.ProviderBase.DbConnectionInternal to);
        internal abstract bool SetInnerConnectionFrom(DbConnection owningObject, System.Data.ProviderBase.DbConnectionInternal to, System.Data.ProviderBase.DbConnectionInternal from);
        internal abstract void SetInnerConnectionTo(DbConnection owningObject, System.Data.ProviderBase.DbConnectionInternal to);

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolCounters PerformanceCounters
        {
            get
            {
                return this._performanceCounters;
            }
        }

        public abstract DbProviderFactory ProviderFactory { get; }
    }
}

