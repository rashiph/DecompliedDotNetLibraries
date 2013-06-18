namespace System.Data.ProviderBase
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Data.Common;

    internal sealed class DbConnectionPoolGroup
    {
        private readonly System.Data.Common.DbConnectionOptions _connectionOptions;
        private System.Data.ProviderBase.DbMetaDataFactory _metaDataFactory;
        internal readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;
        private HybridDictionary _poolCollection;
        private int _poolCount;
        private readonly System.Data.ProviderBase.DbConnectionPoolGroupOptions _poolGroupOptions;
        private System.Data.ProviderBase.DbConnectionPoolGroupProviderInfo _providerInfo;
        private int _state;
        private const int PoolGroupStateActive = 1;
        private const int PoolGroupStateDisabled = 4;
        private const int PoolGroupStateIdle = 2;

        internal DbConnectionPoolGroup(System.Data.Common.DbConnectionOptions connectionOptions, System.Data.ProviderBase.DbConnectionPoolGroupOptions poolGroupOptions)
        {
            this._connectionOptions = connectionOptions;
            this._poolGroupOptions = poolGroupOptions;
            this._poolCollection = new HybridDictionary(1, false);
            this._state = 1;
        }

        internal void Clear()
        {
            this.ClearInternal(true);
        }

        private bool ClearInternal(bool clearing)
        {
            lock (this)
            {
                HybridDictionary dictionary2 = this._poolCollection;
                if (0 < dictionary2.Count)
                {
                    HybridDictionary dictionary = new HybridDictionary(dictionary2.Count, false);
                    foreach (DictionaryEntry entry in dictionary2)
                    {
                        if (entry.Value != null)
                        {
                            System.Data.ProviderBase.DbConnectionPool pool = (System.Data.ProviderBase.DbConnectionPool) entry.Value;
                            if (clearing || (!pool.ErrorOccurred && (pool.Count == 0)))
                            {
                                System.Data.ProviderBase.DbConnectionFactory connectionFactory = pool.ConnectionFactory;
                                connectionFactory.PerformanceCounters.NumberOfActiveConnectionPools.Decrement();
                                connectionFactory.QueuePoolForRelease(pool, clearing);
                            }
                            else
                            {
                                dictionary.Add(entry.Key, entry.Value);
                            }
                        }
                    }
                    this._poolCollection = dictionary;
                    this._poolCount = dictionary.Count;
                }
                if (!clearing && (this._poolCount == 0))
                {
                    if (1 == this._state)
                    {
                        this._state = 2;
                        Bid.Trace("<prov.DbConnectionPoolGroup.ClearInternal|RES|INFO|CPOOL> %d#, Idle\n", this.ObjectID);
                    }
                    else if (2 == this._state)
                    {
                        this._state = 4;
                        Bid.Trace("<prov.DbConnectionPoolGroup.ReadyToRemove|RES|INFO|CPOOL> %d#, Disabled\n", this.ObjectID);
                    }
                }
                return (4 == this._state);
            }
        }

        internal System.Data.ProviderBase.DbConnectionPool GetConnectionPool(System.Data.ProviderBase.DbConnectionFactory connectionFactory)
        {
            object obj2 = null;
            if (this._poolGroupOptions != null)
            {
                System.Data.ProviderBase.DbConnectionPoolIdentity noIdentity = System.Data.ProviderBase.DbConnectionPoolIdentity.NoIdentity;
                if (this._poolGroupOptions.PoolByIdentity)
                {
                    noIdentity = System.Data.ProviderBase.DbConnectionPoolIdentity.GetCurrent();
                    if (noIdentity.IsRestricted)
                    {
                        noIdentity = null;
                    }
                }
                if (noIdentity != null)
                {
                    obj2 = this._poolCollection[noIdentity];
                    if (obj2 == null)
                    {
                        System.Data.ProviderBase.DbConnectionPoolProviderInfo connectionPoolProviderInfo = connectionFactory.CreateConnectionPoolProviderInfo(this.ConnectionOptions);
                        System.Data.ProviderBase.DbConnectionPool pool = new System.Data.ProviderBase.DbConnectionPool(connectionFactory, this, noIdentity, connectionPoolProviderInfo);
                        lock (this)
                        {
                            HybridDictionary dictionary = this._poolCollection;
                            obj2 = dictionary[noIdentity];
                            if ((obj2 == null) && this.MarkPoolGroupAsActive())
                            {
                                pool.Startup();
                                HybridDictionary dictionary2 = new HybridDictionary(1 + dictionary.Count, false);
                                foreach (DictionaryEntry entry in dictionary)
                                {
                                    dictionary2.Add(entry.Key, entry.Value);
                                }
                                dictionary2.Add(noIdentity, pool);
                                connectionFactory.PerformanceCounters.NumberOfActiveConnectionPools.Increment();
                                this._poolCollection = dictionary2;
                                this._poolCount = dictionary2.Count;
                                obj2 = pool;
                                pool = null;
                            }
                        }
                        if (pool != null)
                        {
                            pool.Shutdown();
                        }
                    }
                }
            }
            if (obj2 == null)
            {
                lock (this)
                {
                    this.MarkPoolGroupAsActive();
                }
            }
            return (System.Data.ProviderBase.DbConnectionPool) obj2;
        }

        private bool MarkPoolGroupAsActive()
        {
            if (2 == this._state)
            {
                this._state = 1;
                Bid.Trace("<prov.DbConnectionPoolGroup.ClearInternal|RES|INFO|CPOOL> %d#, Active\n", this.ObjectID);
            }
            return (1 == this._state);
        }

        internal bool Prune()
        {
            return this.ClearInternal(false);
        }

        internal System.Data.Common.DbConnectionOptions ConnectionOptions
        {
            get
            {
                return this._connectionOptions;
            }
        }

        internal int Count
        {
            get
            {
                return this._poolCount;
            }
        }

        internal bool IsDisabled
        {
            get
            {
                return (4 == this._state);
            }
        }

        internal System.Data.ProviderBase.DbMetaDataFactory MetaDataFactory
        {
            get
            {
                return this._metaDataFactory;
            }
            set
            {
                this._metaDataFactory = value;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolGroupOptions PoolGroupOptions
        {
            get
            {
                return this._poolGroupOptions;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolGroupProviderInfo ProviderInfo
        {
            get
            {
                return this._providerInfo;
            }
            set
            {
                this._providerInfo = value;
                if (value != null)
                {
                    this._providerInfo.PoolGroup = this;
                }
            }
        }
    }
}

