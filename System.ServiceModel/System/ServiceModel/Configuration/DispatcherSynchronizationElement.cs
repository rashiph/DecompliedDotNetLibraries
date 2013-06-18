namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed class DispatcherSynchronizationElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            DispatcherSynchronizationElement element = (DispatcherSynchronizationElement) from;
            this.AsynchronousSendEnabled = element.AsynchronousSendEnabled;
            this.MaxPendingReceives = element.MaxPendingReceives;
        }

        protected internal override object CreateBehavior()
        {
            return new DispatcherSynchronizationBehavior(this.AsynchronousSendEnabled, this.MaxPendingReceives);
        }

        [ConfigurationProperty("asynchronousSendEnabled", DefaultValue=false)]
        public bool AsynchronousSendEnabled
        {
            get
            {
                return (bool) base["asynchronousSendEnabled"];
            }
            set
            {
                base["asynchronousSendEnabled"] = value;
            }
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(DispatcherSynchronizationBehavior);
            }
        }

        [ConfigurationProperty("maxPendingReceives", DefaultValue=1), IntegerValidator(MinValue=1)]
        public int MaxPendingReceives
        {
            get
            {
                return (int) base["maxPendingReceives"];
            }
            set
            {
                base["maxPendingReceives"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("asynchronousSendEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxPendingReceives", typeof(int), 1, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

