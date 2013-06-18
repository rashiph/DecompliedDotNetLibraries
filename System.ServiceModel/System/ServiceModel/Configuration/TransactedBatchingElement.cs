namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed class TransactedBatchingElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            TransactedBatchingElement element = from as TransactedBatchingElement;
            this.MaxBatchSize = element.MaxBatchSize;
        }

        protected internal override object CreateBehavior()
        {
            return new TransactedBatchingBehavior(this.MaxBatchSize);
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(TransactedBatchingBehavior);
            }
        }

        [ConfigurationProperty("maxBatchSize", DefaultValue=0), IntegerValidator(MinValue=0)]
        public int MaxBatchSize
        {
            get
            {
                return (int) base["maxBatchSize"];
            }
            set
            {
                base["maxBatchSize"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("maxBatchSize", typeof(int), 0, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

