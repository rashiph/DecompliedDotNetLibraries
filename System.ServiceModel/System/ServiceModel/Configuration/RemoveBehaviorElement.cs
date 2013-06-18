namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class RemoveBehaviorElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            RemoveBehaviorElement element = (RemoveBehaviorElement) from;
            this.Name = element.Name;
        }

        protected internal override object CreateBehavior()
        {
            return null;
        }

        public override Type BehaviorType
        {
            get
            {
                return null;
            }
        }

        [ConfigurationProperty("name", Options=ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("name", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

