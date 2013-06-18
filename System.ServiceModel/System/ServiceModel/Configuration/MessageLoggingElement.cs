namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class MessageLoggingElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [ConfigurationProperty("filters", DefaultValue=null)]
        public XPathMessageFilterElementCollection Filters
        {
            get
            {
                return (XPathMessageFilterElementCollection) base["filters"];
            }
        }

        [ConfigurationProperty("logEntireMessage", DefaultValue=false)]
        public bool LogEntireMessage
        {
            get
            {
                return (bool) base["logEntireMessage"];
            }
            set
            {
                base["logEntireMessage"] = value;
            }
        }

        [ConfigurationProperty("logKnownPii", DefaultValue=false)]
        public bool LogKnownPii
        {
            get
            {
                return (bool) base["logKnownPii"];
            }
            set
            {
                base["logKnownPii"] = value;
            }
        }

        [ConfigurationProperty("logMalformedMessages", DefaultValue=false)]
        public bool LogMalformedMessages
        {
            get
            {
                return (bool) base["logMalformedMessages"];
            }
            set
            {
                base["logMalformedMessages"] = value;
            }
        }

        [ConfigurationProperty("logMessagesAtServiceLevel", DefaultValue=false)]
        public bool LogMessagesAtServiceLevel
        {
            get
            {
                return (bool) base["logMessagesAtServiceLevel"];
            }
            set
            {
                base["logMessagesAtServiceLevel"] = value;
            }
        }

        [ConfigurationProperty("logMessagesAtTransportLevel", DefaultValue=false)]
        public bool LogMessagesAtTransportLevel
        {
            get
            {
                return (bool) base["logMessagesAtTransportLevel"];
            }
            set
            {
                base["logMessagesAtTransportLevel"] = value;
            }
        }

        [ConfigurationProperty("maxMessagesToLog", DefaultValue=0x2710), IntegerValidator(MinValue=-1)]
        public int MaxMessagesToLog
        {
            get
            {
                return (int) base["maxMessagesToLog"];
            }
            set
            {
                base["maxMessagesToLog"] = value;
            }
        }

        [IntegerValidator(MinValue=-1), ConfigurationProperty("maxSizeOfMessageToLog", DefaultValue=0x40000)]
        public int MaxSizeOfMessageToLog
        {
            get
            {
                return (int) base["maxSizeOfMessageToLog"];
            }
            set
            {
                base["maxSizeOfMessageToLog"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("logEntireMessage", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("logKnownPii", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("logMalformedMessages", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("logMessagesAtServiceLevel", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("logMessagesAtTransportLevel", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxMessagesToLog", typeof(int), 0x2710, null, new IntegerValidator(-1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxSizeOfMessageToLog", typeof(int), 0x40000, null, new IntegerValidator(-1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("filters", typeof(XPathMessageFilterElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

