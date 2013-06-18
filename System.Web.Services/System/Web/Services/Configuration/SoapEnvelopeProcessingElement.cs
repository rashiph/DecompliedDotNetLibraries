namespace System.Web.Services.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;

    public sealed class SoapEnvelopeProcessingElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;
        private readonly ConfigurationProperty readTimeout;
        private readonly ConfigurationProperty strict;

        public SoapEnvelopeProcessingElement()
        {
            this.properties = new ConfigurationPropertyCollection();
            this.readTimeout = new ConfigurationProperty("readTimeout", typeof(int), 0x7fffffff, new InfiniteIntConverter(), null, ConfigurationPropertyOptions.None);
            this.strict = new ConfigurationProperty("strict", typeof(bool), false);
            this.properties.Add(this.readTimeout);
            this.properties.Add(this.strict);
        }

        public SoapEnvelopeProcessingElement(int readTimeout) : this()
        {
            this.ReadTimeout = readTimeout;
        }

        public SoapEnvelopeProcessingElement(int readTimeout, bool strict) : this()
        {
            this.ReadTimeout = readTimeout;
            this.IsStrict = strict;
        }

        [ConfigurationProperty("strict", DefaultValue=false)]
        public bool IsStrict
        {
            get
            {
                return (bool) base[this.strict];
            }
            set
            {
                base[this.strict] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }

        [TypeConverter(typeof(InfiniteIntConverter)), ConfigurationProperty("readTimeout", DefaultValue=0x7fffffff)]
        public int ReadTimeout
        {
            get
            {
                return (int) base[this.readTimeout];
            }
            set
            {
                base[this.readTimeout] = value;
            }
        }
    }
}

