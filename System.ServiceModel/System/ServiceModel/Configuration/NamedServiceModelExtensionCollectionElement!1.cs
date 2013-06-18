namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public abstract class NamedServiceModelExtensionCollectionElement<TServiceModelExtensionElement> : ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> where TServiceModelExtensionElement: ServiceModelExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        internal NamedServiceModelExtensionCollectionElement(string extensionCollectionName, string name) : base(extensionCollectionName)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.Name = name;
            }
            else
            {
                this.Name = string.Empty;
            }
        }

        [ConfigurationProperty("name", Options=ConfigurationPropertyOptions.IsKey), StringValidator(MinLength=0)]
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
                base.SetIsModified();
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = base.Properties;
                    this.properties.Add(new ConfigurationProperty("name", typeof(string), null, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                }
                return this.properties;
            }
        }
    }
}

