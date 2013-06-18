namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class StandardBindingOptionalReliableSessionElement : StandardBindingReliableSessionElement
    {
        private ConfigurationPropertyCollection properties;

        public void ApplyConfiguration(OptionalReliableSession optionalReliableSession)
        {
            if (optionalReliableSession == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalReliableSession");
            }
            base.ApplyConfiguration(optionalReliableSession);
            optionalReliableSession.Enabled = this.Enabled;
        }

        public void InitializeFrom(OptionalReliableSession optionalReliableSession)
        {
            if (optionalReliableSession == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalReliableSession");
            }
            base.InitializeFrom(optionalReliableSession);
            this.Enabled = optionalReliableSession.Enabled;
        }

        [ConfigurationProperty("enabled", DefaultValue=false)]
        public bool Enabled
        {
            get
            {
                return (bool) base["enabled"];
            }
            set
            {
                base["enabled"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("enabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

