namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Activities.Tracking;
    using System.Configuration;
    using System.ServiceModel.Configuration;

    public class ProfileElement : TrackingConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public override object ElementKey
        {
            get
            {
                return this.Name;
            }
        }

        [ServiceModelEnumValidator(typeof(ImplementationVisibilityHelper)), ConfigurationProperty("implementationVisibility", DefaultValue=0)]
        public System.Activities.Tracking.ImplementationVisibility ImplementationVisibility
        {
            get
            {
                return (System.Activities.Tracking.ImplementationVisibility) base["implementationVisibility"];
            }
            set
            {
                base["implementationVisibility"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("name", IsKey=true, IsRequired=true)]
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
                    propertys.Add(new ConfigurationProperty("name", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("implementationVisibility", typeof(System.Activities.Tracking.ImplementationVisibility), System.Activities.Tracking.ImplementationVisibility.RootScope, null, new ServiceModelEnumValidator(typeof(ImplementationVisibilityHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("", typeof(ProfileWorkflowElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public ProfileWorkflowElementCollection Workflows
        {
            get
            {
                return (ProfileWorkflowElementCollection) base[""];
            }
        }
    }
}

