namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.Configuration;

    public sealed class DateTimeSerializationSection : ConfigurationSection
    {
        private readonly ConfigurationProperty mode = new ConfigurationProperty("mode", typeof(DateTimeSerializationMode), DateTimeSerializationMode.Roundtrip, new EnumConverter(typeof(DateTimeSerializationMode)), null, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public DateTimeSerializationSection()
        {
            this.properties.Add(this.mode);
        }

        [ConfigurationProperty("mode", DefaultValue=1)]
        public DateTimeSerializationMode Mode
        {
            get
            {
                return (DateTimeSerializationMode) base[this.mode];
            }
            set
            {
                base[this.mode] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        public enum DateTimeSerializationMode
        {
            Default,
            Roundtrip,
            Local
        }
    }
}

