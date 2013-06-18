namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Collections;

    public sealed class SendMessageChannelCache : IDisposable, ICancelable
    {
        private bool allowUnsafeCaching;
        private ChannelCacheSettings channelCacheSettings;
        private static Func<SendMessageChannelCache> defaultExtensionProvider = new Func<SendMessageChannelCache>(SendMessageChannelCache.CreateDefaultExtension);
        private ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> factoryCache;
        private ChannelCacheSettings factoryCacheSettings;
        private bool isDisposed;
        private bool isReadOnly;
        private object thisLock;

        public SendMessageChannelCache() : this(null, null, false)
        {
        }

        public SendMessageChannelCache(ChannelCacheSettings factorySettings, ChannelCacheSettings channelSettings) : this(factorySettings, channelSettings, false)
        {
        }

        public SendMessageChannelCache(ChannelCacheSettings factorySettings, ChannelCacheSettings channelSettings, bool allowUnsafeCaching)
        {
            this.allowUnsafeCaching = allowUnsafeCaching;
            this.FactorySettings = factorySettings;
            this.ChannelSettings = channelSettings;
            this.thisLock = new object();
        }

        private static SendMessageChannelCache CreateDefaultExtension()
        {
            return new SendMessageChannelCache { FactorySettings = { LeaseTimeout = ChannelCacheDefaults.DefaultFactoryLeaseTimeout }, ChannelSettings = { LeaseTimeout = ChannelCacheDefaults.DefaultChannelLeaseTimeout } };
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                lock (this.thisLock)
                {
                    if (!this.isDisposed)
                    {
                        if (this.factoryCache != null)
                        {
                            this.factoryCache.Dispose();
                        }
                        this.isDisposed = true;
                    }
                }
            }
        }

        internal ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> GetFactoryCache()
        {
            if (this.factoryCache == null)
            {
                this.isReadOnly = true;
                lock (this.thisLock)
                {
                    this.ThrowIfDisposed();
                    if (this.factoryCache == null)
                    {
                        ObjectCacheSettings settings = new ObjectCacheSettings {
                            CacheLimit = this.FactorySettings.MaxItemsInCache,
                            IdleTimeout = this.FactorySettings.IdleTimeout,
                            LeaseTimeout = this.FactorySettings.LeaseTimeout
                        };
                        this.factoryCache = new ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference>(settings);
                    }
                }
            }
            return this.factoryCache;
        }

        void ICancelable.Cancel()
        {
            this.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(typeof(SendMessageChannelCache).ToString()));
            }
        }

        private void ThrowIfReadOnly()
        {
            if (this.isReadOnly)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CacheSettingsLocked));
            }
        }

        public bool AllowUnsafeCaching
        {
            get
            {
                return this.allowUnsafeCaching;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.allowUnsafeCaching = value;
            }
        }

        public ChannelCacheSettings ChannelSettings
        {
            get
            {
                return this.channelCacheSettings;
            }
            set
            {
                this.ThrowIfReadOnly();
                if (value == null)
                {
                    ChannelCacheSettings settings = new ChannelCacheSettings {
                        LeaseTimeout = ChannelCacheDefaults.DefaultChannelLeaseTimeout
                    };
                    this.channelCacheSettings = settings;
                }
                else
                {
                    this.channelCacheSettings = value;
                }
            }
        }

        internal static Func<SendMessageChannelCache> DefaultExtensionProvider
        {
            get
            {
                return defaultExtensionProvider;
            }
        }

        public ChannelCacheSettings FactorySettings
        {
            get
            {
                return this.factoryCacheSettings;
            }
            set
            {
                this.ThrowIfReadOnly();
                if (value == null)
                {
                    ChannelCacheSettings settings = new ChannelCacheSettings {
                        LeaseTimeout = ChannelCacheDefaults.DefaultFactoryLeaseTimeout
                    };
                    this.factoryCacheSettings = settings;
                }
                else
                {
                    this.factoryCacheSettings = value;
                }
            }
        }
    }
}

