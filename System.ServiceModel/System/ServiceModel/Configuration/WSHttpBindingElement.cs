namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class WSHttpBindingElement : WSHttpBindingBaseElement
    {
        private ConfigurationPropertyCollection properties;

        public WSHttpBindingElement() : this(null)
        {
        }

        public WSHttpBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            WSHttpBinding binding2 = (WSHttpBinding) binding;
            this.AllowCookies = binding2.AllowCookies;
            this.Security.InitializeFrom(binding2.Security);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            WSHttpBinding binding2 = (WSHttpBinding) binding;
            binding2.AllowCookies = this.AllowCookies;
            this.Security.ApplyConfiguration(binding2.Security);
        }

        [ConfigurationProperty("allowCookies", DefaultValue=false)]
        public bool AllowCookies
        {
            get
            {
                return (bool) base["allowCookies"];
            }
            set
            {
                base["allowCookies"] = value;
            }
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(WSHttpBinding);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("allowCookies", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(WSHttpSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("security")]
        public WSHttpSecurityElement Security
        {
            get
            {
                return (WSHttpSecurityElement) base["security"];
            }
        }
    }
}

