namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net;

    public sealed class HttpWebRequestElement : ConfigurationElement
    {
        private readonly ConfigurationProperty maximumErrorResponseLength = new ConfigurationProperty("maximumErrorResponseLength", typeof(int), 0x40, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty maximumResponseHeadersLength = new ConfigurationProperty("maximumResponseHeadersLength", typeof(int), 0x40, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty maximumUnauthorizedUploadLength = new ConfigurationProperty("maximumUnauthorizedUploadLength", typeof(int), -1, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty useUnsafeHeaderParsing = new ConfigurationProperty("useUnsafeHeaderParsing", typeof(bool), false, ConfigurationPropertyOptions.None);

        public HttpWebRequestElement()
        {
            this.properties.Add(this.maximumResponseHeadersLength);
            this.properties.Add(this.maximumErrorResponseLength);
            this.properties.Add(this.maximumUnauthorizedUploadLength);
            this.properties.Add(this.useUnsafeHeaderParsing);
        }

        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel)
            {
                PropertyInformation[] informationArray = new PropertyInformation[] { base.ElementInformation.Properties["maximumResponseHeadersLength"], base.ElementInformation.Properties["maximumErrorResponseLength"] };
                foreach (PropertyInformation information in informationArray)
                {
                    if (information.ValueOrigin == PropertyValueOrigin.SetHere)
                    {
                        try
                        {
                            ExceptionHelper.WebPermissionUnrestricted.Demand();
                        }
                        catch (Exception exception)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("net_config_property_permission", new object[] { information.Name }), exception);
                        }
                    }
                }
            }
        }

        [ConfigurationProperty("maximumErrorResponseLength", DefaultValue=0x40)]
        public int MaximumErrorResponseLength
        {
            get
            {
                return (int) base[this.maximumErrorResponseLength];
            }
            set
            {
                base[this.maximumErrorResponseLength] = value;
            }
        }

        [ConfigurationProperty("maximumResponseHeadersLength", DefaultValue=0x40)]
        public int MaximumResponseHeadersLength
        {
            get
            {
                return (int) base[this.maximumResponseHeadersLength];
            }
            set
            {
                base[this.maximumResponseHeadersLength] = value;
            }
        }

        [ConfigurationProperty("maximumUnauthorizedUploadLength", DefaultValue=-1)]
        public int MaximumUnauthorizedUploadLength
        {
            get
            {
                return (int) base[this.maximumUnauthorizedUploadLength];
            }
            set
            {
                base[this.maximumUnauthorizedUploadLength] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("useUnsafeHeaderParsing", DefaultValue=false)]
        public bool UseUnsafeHeaderParsing
        {
            get
            {
                return (bool) base[this.useUnsafeHeaderParsing];
            }
            set
            {
                base[this.useUnsafeHeaderParsing] = value;
            }
        }
    }
}

