namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed class ServiceMetadataPublishingElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ServiceMetadataPublishingElement element = (ServiceMetadataPublishingElement) from;
            this.HttpGetEnabled = element.HttpGetEnabled;
            this.HttpGetUrl = element.HttpGetUrl;
            this.HttpsGetEnabled = element.HttpsGetEnabled;
            this.HttpsGetUrl = element.HttpsGetUrl;
            this.ExternalMetadataLocation = element.ExternalMetadataLocation;
            this.PolicyVersion = element.PolicyVersion;
            this.HttpGetBinding = element.HttpGetBinding;
            this.HttpGetBindingConfiguration = element.HttpGetBindingConfiguration;
            this.HttpsGetBinding = element.HttpsGetBinding;
            this.HttpsGetBindingConfiguration = element.HttpsGetBindingConfiguration;
        }

        protected internal override object CreateBehavior()
        {
            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior {
                HttpGetEnabled = this.HttpGetEnabled,
                HttpGetUrl = this.HttpGetUrl,
                HttpsGetEnabled = this.HttpsGetEnabled,
                HttpsGetUrl = this.HttpsGetUrl,
                ExternalMetadataLocation = this.ExternalMetadataLocation
            };
            behavior.MetadataExporter.PolicyVersion = this.PolicyVersion;
            if (!string.IsNullOrEmpty(this.HttpGetBinding))
            {
                behavior.HttpGetBinding = ConfigLoader.LookupBinding(this.HttpGetBinding, this.HttpGetBindingConfiguration);
            }
            if (!string.IsNullOrEmpty(this.HttpsGetBinding))
            {
                behavior.HttpsGetBinding = ConfigLoader.LookupBinding(this.HttpsGetBinding, this.HttpsGetBindingConfiguration);
            }
            return behavior;
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceMetadataBehavior);
            }
        }

        [ConfigurationProperty("externalMetadataLocation")]
        public Uri ExternalMetadataLocation
        {
            get
            {
                return (Uri) base["externalMetadataLocation"];
            }
            set
            {
                base["externalMetadataLocation"] = value;
            }
        }

        [ConfigurationProperty("httpGetBinding", DefaultValue=""), StringValidator(MinLength=0)]
        public string HttpGetBinding
        {
            get
            {
                return (string) base["httpGetBinding"];
            }
            set
            {
                base["httpGetBinding"] = value;
            }
        }

        [ConfigurationProperty("httpGetBindingConfiguration", DefaultValue=""), StringValidator(MinLength=0)]
        public string HttpGetBindingConfiguration
        {
            get
            {
                return (string) base["httpGetBindingConfiguration"];
            }
            set
            {
                base["httpGetBindingConfiguration"] = value;
            }
        }

        [ConfigurationProperty("httpGetEnabled", DefaultValue=false)]
        public bool HttpGetEnabled
        {
            get
            {
                return (bool) base["httpGetEnabled"];
            }
            set
            {
                base["httpGetEnabled"] = value;
            }
        }

        [ConfigurationProperty("httpGetUrl")]
        public Uri HttpGetUrl
        {
            get
            {
                return (Uri) base["httpGetUrl"];
            }
            set
            {
                base["httpGetUrl"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("httpsGetBinding", DefaultValue="")]
        public string HttpsGetBinding
        {
            get
            {
                return (string) base["httpsGetBinding"];
            }
            set
            {
                base["httpsGetBinding"] = value;
            }
        }

        [ConfigurationProperty("httpsGetBindingConfiguration", DefaultValue=""), StringValidator(MinLength=0)]
        public string HttpsGetBindingConfiguration
        {
            get
            {
                return (string) base["httpsGetBindingConfiguration"];
            }
            set
            {
                base["httpsGetBindingConfiguration"] = value;
            }
        }

        [ConfigurationProperty("httpsGetEnabled", DefaultValue=false)]
        public bool HttpsGetEnabled
        {
            get
            {
                return (bool) base["httpsGetEnabled"];
            }
            set
            {
                base["httpsGetEnabled"] = value;
            }
        }

        [ConfigurationProperty("httpsGetUrl")]
        public Uri HttpsGetUrl
        {
            get
            {
                return (Uri) base["httpsGetUrl"];
            }
            set
            {
                base["httpsGetUrl"] = value;
            }
        }

        [TypeConverter(typeof(PolicyVersionConverter)), ConfigurationProperty("policyVersion", DefaultValue="Default")]
        public System.ServiceModel.Description.PolicyVersion PolicyVersion
        {
            get
            {
                return (System.ServiceModel.Description.PolicyVersion) base["policyVersion"];
            }
            set
            {
                base["policyVersion"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("externalMetadataLocation", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpGetEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpGetUrl", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpsGetEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpsGetUrl", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpGetBinding", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpGetBindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpsGetBinding", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("httpsGetBindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("policyVersion", typeof(System.ServiceModel.Description.PolicyVersion), "Default", new PolicyVersionConverter(), null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

