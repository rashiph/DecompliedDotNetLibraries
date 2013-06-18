namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public sealed class SendMessageChannelCacheElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        protected internal override object CreateBehavior()
        {
            SendMessageChannelCacheBehavior behavior = new SendMessageChannelCacheBehavior {
                AllowUnsafeCaching = this.AllowUnsafeCaching
            };
            ChannelCacheSettings settings = new ChannelCacheSettings {
                IdleTimeout = this.FactorySettings.IdleTimeout,
                LeaseTimeout = this.FactorySettings.LeaseTimeout,
                MaxItemsInCache = this.FactorySettings.MaxItemsInCache
            };
            behavior.FactorySettings = settings;
            ChannelCacheSettings settings2 = new ChannelCacheSettings {
                IdleTimeout = this.ChannelSettings.IdleTimeout,
                LeaseTimeout = this.ChannelSettings.LeaseTimeout,
                MaxItemsInCache = this.ChannelSettings.MaxItemsInCache
            };
            behavior.ChannelSettings = settings2;
            return behavior;
        }

        [ConfigurationProperty("allowUnsafeCaching", DefaultValue=false)]
        public bool AllowUnsafeCaching
        {
            get
            {
                return (bool) base["allowUnsafeCaching"];
            }
            set
            {
                base["allowUnsafeCaching"] = value;
            }
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(SendMessageChannelCacheBehavior);
            }
        }

        [ConfigurationProperty("channelSettings")]
        public ChannelSettingsElement ChannelSettings
        {
            get
            {
                return (ChannelSettingsElement) base["channelSettings"];
            }
        }

        [ConfigurationProperty("factorySettings")]
        public FactorySettingsElement FactorySettings
        {
            get
            {
                return (FactorySettingsElement) base["factorySettings"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("allowUnsafeCaching", typeof(bool), false));
                    propertys.Add(new ConfigurationProperty("factorySettings", typeof(FactorySettingsElement)));
                    propertys.Add(new ConfigurationProperty("channelSettings", typeof(ChannelSettingsElement)));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

