namespace System.Data.ProviderBase
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class DbConnectionFactory
    {
        private Dictionary<string, DbConnectionPoolGroup> _connectionPoolGroups;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        private readonly DbConnectionPoolCounters _performanceCounters;
        private readonly List<DbConnectionPoolGroup> _poolGroupsToRelease;
        private readonly List<DbConnectionPool> _poolsToRelease;
        private readonly Timer _pruningTimer;
        private const int PruningDueTime = 0x3a980;
        private const int PruningPeriod = 0x7530;

        protected DbConnectionFactory() : this(DbConnectionPoolCountersNoCounters.SingletonInstance)
        {
        }

        protected DbConnectionFactory(DbConnectionPoolCounters performanceCounters)
        {
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this._performanceCounters = performanceCounters;
            this._connectionPoolGroups = new Dictionary<string, DbConnectionPoolGroup>();
            this._poolsToRelease = new List<DbConnectionPool>();
            this._poolGroupsToRelease = new List<DbConnectionPoolGroup>();
            this._pruningTimer = this.CreatePruningTimer();
        }

        public void ClearAllPools()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<prov.DbConnectionFactory.ClearAllPools|API> ");
            try
            {
                foreach (KeyValuePair<string, DbConnectionPoolGroup> pair in this._connectionPoolGroups)
                {
                    DbConnectionPoolGroup group = pair.Value;
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
            ADP.CheckArgumentNull(connection, "connection");
            Bid.ScopeEnter(out ptr, "<prov.DbConnectionFactory.ClearPool|API> %d#", this.GetObjectId(connection));
            try
            {
                DbConnectionPoolGroup connectionPoolGroup = this.GetConnectionPoolGroup(connection);
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

        public void ClearPool(string connectionString)
        {
            IntPtr ptr;
            ADP.CheckArgumentNull(connectionString, "connectionString");
            Bid.ScopeEnter(out ptr, "<prov.DbConnectionFactory.ClearPool|API> connectionString");
            try
            {
                DbConnectionPoolGroup group;
                if (this._connectionPoolGroups.TryGetValue(connectionString, out group))
                {
                    group.Clear();
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        protected abstract DbConnectionInternal CreateConnection(DbConnectionOptions options, object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningConnection);
        protected abstract DbConnectionOptions CreateConnectionOptions(string connectionString, DbConnectionOptions previous);
        protected abstract DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(DbConnectionOptions options);
        internal virtual DbConnectionPoolGroupProviderInfo CreateConnectionPoolGroupProviderInfo(DbConnectionOptions connectionOptions)
        {
            return null;
        }

        internal virtual DbConnectionPoolProviderInfo CreateConnectionPoolProviderInfo(DbConnectionOptions connectionOptions)
        {
            return null;
        }

        protected virtual DbMetaDataFactory CreateMetaDataFactory(DbConnectionInternal internalConnection, out bool cacheMetaDataFactory)
        {
            cacheMetaDataFactory = false;
            throw ADP.NotSupported();
        }

        internal DbConnectionInternal CreateNonPooledConnection(DbConnection owningConnection, DbConnectionPoolGroup poolGroup)
        {
            DbConnectionOptions connectionOptions = poolGroup.ConnectionOptions;
            DbConnectionPoolGroupProviderInfo providerInfo = poolGroup.ProviderInfo;
            DbConnectionInternal internal2 = this.CreateConnection(connectionOptions, providerInfo, null, owningConnection);
            if (internal2 != null)
            {
                this.PerformanceCounters.HardConnectsPerSecond.Increment();
                internal2.MakeNonPooledObject(owningConnection, this.PerformanceCounters);
            }
            Bid.Trace("<prov.DbConnectionFactory.CreateNonPooledConnection|RES|CPOOL> %d#, Non-pooled database connection created.\n", this.ObjectID);
            return internal2;
        }

        internal DbConnectionInternal CreatePooledConnection(DbConnection owningConnection, DbConnectionPool pool, DbConnectionOptions options)
        {
            DbConnectionPoolGroupProviderInfo providerInfo = pool.PoolGroup.ProviderInfo;
            DbConnectionInternal internal2 = this.CreateConnection(options, providerInfo, pool, owningConnection);
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

        protected DbConnectionOptions FindConnectionOptions(string connectionString)
        {
            DbConnectionPoolGroup group;
            if (!ADP.IsEmpty(connectionString) && this._connectionPoolGroups.TryGetValue(connectionString, out group))
            {
                return group.ConnectionOptions;
            }
            return null;
        }

        internal DbConnectionInternal GetConnection(DbConnection owningConnection)
        {
            DbConnectionInternal connection;
            int num2 = 10;
            int millisecondsTimeout = 1;
            do
            {
                DbConnectionPoolGroup connectionPoolGroup = this.GetConnectionPoolGroup(owningConnection);
                DbConnectionPool connectionPool = this.GetConnectionPool(owningConnection, connectionPoolGroup);
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
                            throw ADP.PooledOpenTimeout();
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
                throw ADP.PooledOpenTimeout();
            }
            return connection;
        }

        private DbConnectionPool GetConnectionPool(DbConnection owningObject, DbConnectionPoolGroup connectionPoolGroup)
        {
            if (connectionPoolGroup.IsDisabled && (connectionPoolGroup.PoolGroupOptions != null))
            {
                Bid.Trace("<prov.DbConnectionFactory.GetConnectionPool|RES|INFO|CPOOL> %d#, DisabledPoolGroup=%d#\n", this.ObjectID, connectionPoolGroup.ObjectID);
                DbConnectionPoolGroupOptions poolGroupOptions = connectionPoolGroup.PoolGroupOptions;
                DbConnectionOptions connectionOptions = connectionPoolGroup.ConnectionOptions;
                string connectionString = connectionOptions.UsersConnectionString(false);
                connectionPoolGroup = this.GetConnectionPoolGroup(connectionString, poolGroupOptions, ref connectionOptions);
                this.SetConnectionPoolGroup(owningObject, connectionPoolGroup);
            }
            return connectionPoolGroup.GetConnectionPool(this);
        }

        internal abstract DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection);
        internal DbConnectionPoolGroup GetConnectionPoolGroup(string connectionString, DbConnectionPoolGroupOptions poolOptions, ref DbConnectionOptions userConnectionOptions)
        {
            DbConnectionPoolGroup group;
            if (ADP.IsEmpty(connectionString))
            {
                return null;
            }
            if (!this._connectionPoolGroups.TryGetValue(connectionString, out group) || (group.IsDisabled && (group.PoolGroupOptions != null)))
            {
                DbConnectionOptions options = this.CreateConnectionOptions(connectionString, userConnectionOptions);
                if (options == null)
                {
                    throw ADP.InternalConnectionError(ADP.ConnectionError.ConnectionOptionsMissing);
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
                if ((poolOptions == null) && ADP.IsWindowsNT)
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
                DbConnectionPoolGroup group2 = new DbConnectionPoolGroup(options, poolOptions) {
                    ProviderInfo = this.CreateConnectionPoolGroupProviderInfo(options)
                };
                lock (this)
                {
                    Dictionary<string, DbConnectionPoolGroup> dictionary = this._connectionPoolGroups;
                    if (!dictionary.TryGetValue(str, out group))
                    {
                        Dictionary<string, DbConnectionPoolGroup> dictionary2 = new Dictionary<string, DbConnectionPoolGroup>(1 + dictionary.Count);
                        foreach (KeyValuePair<string, DbConnectionPoolGroup> pair in dictionary)
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

        internal abstract DbConnectionInternal GetInnerConnection(DbConnection connection);
        internal DbMetaDataFactory GetMetaDataFactory(DbConnectionPoolGroup connectionPoolGroup, DbConnectionInternal internalConnection)
        {
            DbMetaDataFactory metaDataFactory = connectionPoolGroup.MetaDataFactory;
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
                    foreach (DbConnectionPool pool in this._poolsToRelease.ToArray())
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
                    foreach (DbConnectionPoolGroup group in this._poolGroupsToRelease.ToArray())
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
                Dictionary<string, DbConnectionPoolGroup> dictionary2 = this._connectionPoolGroups;
                Dictionary<string, DbConnectionPoolGroup> dictionary = new Dictionary<string, DbConnectionPoolGroup>(dictionary2.Count);
                foreach (KeyValuePair<string, DbConnectionPoolGroup> pair in dictionary2)
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

        internal void QueuePoolForRelease(DbConnectionPool pool, bool clearing)
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

        internal void QueuePoolGroupForRelease(DbConnectionPoolGroup poolGroup)
        {
            Bid.Trace("<prov.DbConnectionFactory.QueuePoolGroupForRelease|RES|INFO|CPOOL> %d#, poolGroup=%d#\n", this.ObjectID, poolGroup.ObjectID);
            lock (this._poolGroupsToRelease)
            {
                this._poolGroupsToRelease.Add(poolGroup);
            }
            this.PerformanceCounters.NumberOfInactiveConnectionPoolGroups.Increment();
        }

        internal abstract void SetConnectionPoolGroup(DbConnection outerConnection, DbConnectionPoolGroup poolGroup);
        internal abstract void SetInnerConnectionEvent(DbConnection owningObject, DbConnectionInternal to);
        internal abstract bool SetInnerConnectionFrom(DbConnection owningObject, DbConnectionInternal to, DbConnectionInternal from);
        internal abstract void SetInnerConnectionTo(DbConnection owningObject, DbConnectionInternal to);

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal DbConnectionPoolCounters PerformanceCounters
        {
            get
            {
                return this._performanceCounters;
            }
        }

        public abstract DbProviderFactory ProviderFactory { get; }
    }
}

