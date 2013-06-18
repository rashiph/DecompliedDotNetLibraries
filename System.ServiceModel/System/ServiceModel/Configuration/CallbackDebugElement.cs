namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed class CallbackDebugElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            CallbackDebugElement element = (CallbackDebugElement) from;
            this.IncludeExceptionDetailInFaults = element.IncludeExceptionDetailInFaults;
        }

        protected internal override object CreateBehavior()
        {
            return new CallbackDebugBehavior(this.IncludeExceptionDetailInFaults);
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(CallbackDebugBehavior);
            }
        }

        [ConfigurationProperty("includeExceptionDetailInFaults", DefaultValue=false)]
        public bool IncludeExceptionDetailInFaults
        {
            get
            {
                return (bool) base["includeExceptionDetailInFaults"];
            }
            set
            {
                base["includeExceptionDetailInFaults"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("includeExceptionDetailInFaults", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

