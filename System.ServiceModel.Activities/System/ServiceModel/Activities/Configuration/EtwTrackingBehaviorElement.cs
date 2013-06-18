namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public class EtwTrackingBehaviorElement : BehaviorExtensionElement
    {
        private const string profileNameParameter = "profileName";
        private ConfigurationPropertyCollection properties;

        protected internal override object CreateBehavior()
        {
            return new EtwTrackingBehavior { ProfileName = this.ProfileName };
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(EtwTrackingBehavior);
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("profileName", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey)]
        public string ProfileName
        {
            get
            {
                return (string) base["profileName"];
            }
            set
            {
                base["profileName"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("profileName", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

