namespace Microsoft.Transactions.Bridge.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    internal sealed class TransactionBridgeSection : ConfigurationSection
    {
        private const string dtcGatewayType = "Microsoft.Transactions.Bridge.Dtc.GatewayTransactionManager, Microsoft.Transactions.Bridge.Dtc, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        private ConfigurationPropertyCollection properties;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionBridgeSection() : this(false)
        {
        }

        internal TransactionBridgeSection(bool setDefaults)
        {
            if (setDefaults)
            {
                this.Protocols.SetDefaults();
            }
        }

        internal static TransactionBridgeSection GetSection()
        {
            TransactionBridgeSection section = ConfigurationStrings.GetSection(ConfigurationStrings.GetSectionPath("transactionBridge")) as TransactionBridgeSection;
            if (section == null)
            {
                section = new TransactionBridgeSection(true);
            }
            return section;
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            if (parentElement == null)
            {
                parentElement = new TransactionBridgeSection(true);
            }
            base.Reset(parentElement);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("protocols", typeof(ProtocolElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("transactionManagerType", typeof(string), "Microsoft.Transactions.Bridge.Dtc.GatewayTransactionManager, Microsoft.Transactions.Bridge.Dtc, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("protocols", DefaultValue=null, Options=ConfigurationPropertyOptions.None)]
        public ProtocolElementCollection Protocols
        {
            get
            {
                return (ProtocolElementCollection) base["protocols"];
            }
        }

        [ConfigurationProperty("transactionManagerType", DefaultValue="Microsoft.Transactions.Bridge.Dtc.GatewayTransactionManager, Microsoft.Transactions.Bridge.Dtc, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", Options=ConfigurationPropertyOptions.None), StringValidator(MinLength=0)]
        public string TransactionManagerType
        {
            get
            {
                return (string) base["transactionManagerType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["transactionManagerType"] = value;
            }
        }
    }
}

