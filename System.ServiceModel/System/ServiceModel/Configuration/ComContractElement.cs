namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ComContractElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public ComContractElement()
        {
        }

        public ComContractElement(string contractType) : this()
        {
            this.Contract = contractType;
        }

        [ConfigurationProperty("contract", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string Contract
        {
            get
            {
                return (string) base["contract"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["contract"] = value;
            }
        }

        [ConfigurationProperty("exposedMethods", Options=ConfigurationPropertyOptions.None)]
        public ComMethodElementCollection ExposedMethods
        {
            get
            {
                return (ComMethodElementCollection) base["exposedMethods"];
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("name", DefaultValue="", Options=ConfigurationPropertyOptions.None)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["name"] = value;
            }
        }

        [ConfigurationProperty("namespace", DefaultValue="", Options=ConfigurationPropertyOptions.None), StringValidator(MinLength=0)]
        public string Namespace
        {
            get
            {
                return (string) base["namespace"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["namespace"] = value;
            }
        }

        [ConfigurationProperty("persistableTypes")]
        public ComPersistableTypeElementCollection PersistableTypes
        {
            get
            {
                return (ComPersistableTypeElementCollection) base["persistableTypes"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("contract", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("exposedMethods", typeof(ComMethodElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("name", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("namespace", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("persistableTypes", typeof(ComPersistableTypeElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("requiresSession", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("userDefinedTypes", typeof(ComUdtElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("requiresSession", DefaultValue=true)]
        public bool RequiresSession
        {
            get
            {
                return (bool) base["requiresSession"];
            }
            set
            {
                base["requiresSession"] = value;
            }
        }

        [ConfigurationProperty("userDefinedTypes")]
        public ComUdtElementCollection UserDefinedTypes
        {
            get
            {
                return (ComUdtElementCollection) base["userDefinedTypes"];
            }
        }
    }
}

