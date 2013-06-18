namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    public class StateElement : TrackingConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public override object ElementKey
        {
            get
            {
                return this.Name;
            }
        }

        [ConfigurationProperty("name", IsKey=true, DefaultValue="*"), StringValidator(MinLength=0)]
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
                    propertys.Add(new ConfigurationProperty("name", typeof(string), "*", null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

