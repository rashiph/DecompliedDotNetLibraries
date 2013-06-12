namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class EventMappingSettings : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propEndEventCode = new ConfigurationProperty("endEventCode", typeof(int), 0x7fffffff, null, StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propStartEventCode = new ConfigurationProperty("startEventCode", typeof(int), 0, null, StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), string.Empty, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);
        private System.Type _type;

        static EventMappingSettings()
        {
            _properties.Add(_propName);
            _properties.Add(_propType);
            _properties.Add(_propStartEventCode);
            _properties.Add(_propEndEventCode);
        }

        internal EventMappingSettings()
        {
        }

        public EventMappingSettings(string name, string type) : this()
        {
            this.Name = name;
            this.Type = type;
        }

        public EventMappingSettings(string name, string type, int startEventCode, int endEventCode) : this()
        {
            this.Name = name;
            this.Type = type;
            this.StartEventCode = startEventCode;
            this.EndEventCode = endEventCode;
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("endEventCode", DefaultValue=0x7fffffff)]
        public int EndEventCode
        {
            get
            {
                return (int) base[_propEndEventCode];
            }
            set
            {
                base[_propEndEventCode] = value;
            }
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true, DefaultValue="")]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        internal System.Type RealType
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("startEventCode", DefaultValue=0)]
        public int StartEventCode
        {
            get
            {
                return (int) base[_propStartEventCode];
            }
            set
            {
                base[_propStartEventCode] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired=true, DefaultValue="")]
        public string Type
        {
            get
            {
                return (string) base[_propType];
            }
            set
            {
                base[_propType] = value;
            }
        }
    }
}

