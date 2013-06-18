namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Activities.Tracking;
    using System.Configuration;

    public class ActivityScheduledQueryElement : TrackingQueryElement
    {
        private ConfigurationPropertyCollection properties;

        protected override TrackingQuery NewTrackingQuery()
        {
            return new ActivityScheduledQuery { ActivityName = this.ActivityName, ChildActivityName = this.ChildActivityName };
        }

        [ConfigurationProperty("activityName", IsKey=true, DefaultValue="*"), StringValidator(MinLength=1)]
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

        [ConfigurationProperty("childActivityName", IsKey=true, DefaultValue="*"), StringValidator(MinLength=1)]
        public string ChildActivityName
        {
            get
            {
                return (string) base["childActivityName"];
            }
            set
            {
                base["childActivityName"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("activityName", typeof(string), "*", null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("childActivityName", typeof(string), "*", null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

