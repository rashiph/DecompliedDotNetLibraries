namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.PeerResolvers;

    public sealed class PeerCustomResolverElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(PeerCustomResolverSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            if (this.Address != null)
            {
                settings.Address = new EndpointAddress(this.Address, ConfigLoader.LoadIdentity(this.Identity), this.Headers.Headers);
            }
            settings.BindingSection = this.Binding;
            settings.BindingConfiguration = this.BindingConfiguration;
            if (!string.IsNullOrEmpty(this.Binding) && !string.IsNullOrEmpty(this.BindingConfiguration))
            {
                settings.Binding = ConfigLoader.LookupBinding(this.Binding, this.BindingConfiguration);
            }
            if (!string.IsNullOrEmpty(this.ResolverType))
            {
                Type type = Type.GetType(this.ResolverType, false);
                if (type == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("PeerResolverInvalid", new object[] { this.ResolverType })));
                }
                settings.Resolver = Activator.CreateInstance(type) as PeerResolver;
            }
        }

        internal void InitializeFrom(PeerCustomResolverSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            if (settings.Address != null)
            {
                this.Address = settings.Address.Uri;
                this.Identity.InitializeFrom(settings.Address.Identity);
            }
            if (settings.Resolver != null)
            {
                this.ResolverType = settings.Resolver.GetType().AssemblyQualifiedName;
            }
            if (settings.Binding != null)
            {
                string str;
                this.BindingConfiguration = "PeerCustomResolver" + Guid.NewGuid().ToString();
                BindingsSection.TryAdd(this.BindingConfiguration, settings.Binding, out str);
                this.Binding = str;
            }
        }

        [ConfigurationProperty("address", DefaultValue=null, Options=ConfigurationPropertyOptions.None)]
        public Uri Address
        {
            get
            {
                return (Uri) base["address"];
            }
            set
            {
                base["address"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("binding", DefaultValue="")]
        public string Binding
        {
            get
            {
                return (string) base["binding"];
            }
            set
            {
                base["binding"] = value;
            }
        }

        [ConfigurationProperty("bindingConfiguration", DefaultValue=""), StringValidator(MinLength=0)]
        public string BindingConfiguration
        {
            get
            {
                return (string) base["bindingConfiguration"];
            }
            set
            {
                base["bindingConfiguration"] = value;
            }
        }

        [ConfigurationProperty("headers")]
        public AddressHeaderCollectionElement Headers
        {
            get
            {
                return (AddressHeaderCollectionElement) base["headers"];
            }
        }

        [ConfigurationProperty("identity")]
        public IdentityElement Identity
        {
            get
            {
                return (IdentityElement) base["identity"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("address", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("headers", typeof(AddressHeaderCollectionElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("identity", typeof(IdentityElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("binding", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("bindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("resolverType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("resolverType", DefaultValue="")]
        public string ResolverType
        {
            get
            {
                return (string) base["resolverType"];
            }
            set
            {
                base["resolverType"] = value;
            }
        }
    }
}

