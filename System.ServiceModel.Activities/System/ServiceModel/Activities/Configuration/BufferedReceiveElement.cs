namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public sealed class BufferedReceiveElement : BehaviorExtensionElement
    {
        private const string MaxPendingMessagesPerChannelString = "maxPendingMessagesPerChannel";
        private ConfigurationPropertyCollection properties;

        protected internal override object CreateBehavior()
        {
            return new BufferedReceiveServiceBehavior { MaxPendingMessagesPerChannel = this.MaxPendingMessagesPerChannel };
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(BufferedReceiveServiceBehavior);
            }
        }

        [ConfigurationProperty("maxPendingMessagesPerChannel", DefaultValue=0x200), TypeConverter(typeof(Int32Converter)), IntegerValidator(MinValue=1, MaxValue=0x7fffffff)]
        public int MaxPendingMessagesPerChannel
        {
            get
            {
                return (int) base["maxPendingMessagesPerChannel"];
            }
            set
            {
                base["maxPendingMessagesPerChannel"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("maxPendingMessagesPerChannel", typeof(int), 0x200, new Int32Converter(), new IntegerValidator(1, 0x7fffffff), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

