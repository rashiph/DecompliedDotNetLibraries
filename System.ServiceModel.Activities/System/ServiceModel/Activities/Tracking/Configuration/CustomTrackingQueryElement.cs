namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Activities.Tracking;
    using System.Configuration;

    public class CustomTrackingQueryElement : TrackingQueryElement
    {
        private ConfigurationPropertyCollection properties;

        protected override TrackingQuery NewTrackingQuery()
        {
            return new CustomTrackingQuery { Name = this.Name, ActivityName = this.ActivityName };
        }

        [StringValidator(MinLength=1), ConfigurationProperty("activityName", IsKey=true, DefaultValue="*")]
        public string ActivityName
        {
            get
            {
                return (string) base["activityName"];
            }
            set
            {
                base["activityName"] = value;
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
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("name", typeof(string), "*", null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("activityName", typeof(string), "*", null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

