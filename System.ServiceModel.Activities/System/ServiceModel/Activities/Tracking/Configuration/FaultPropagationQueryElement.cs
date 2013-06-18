namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Activities.Tracking;
    using System.Configuration;

    public class FaultPropagationQueryElement : TrackingQueryElement
    {
        private ConfigurationPropertyCollection properties;

        protected override TrackingQuery NewTrackingQuery()
        {
            return new FaultPropagationQuery { FaultSourceActivityName = this.FaultSourceActivityName, FaultHandlerActivityName = this.FaultHandlerActivityName };
        }

        [ConfigurationProperty("faultHandlerActivityName", IsKey=true, DefaultValue="*"), StringValidator(MinLength=1)]
        public string FaultHandlerActivityName
        {
            get
            {
                return (string) base["faultHandlerActivityName"];
            }
            set
            {
                base["faultHandlerActivityName"] = value;
            }
        }

        [ConfigurationProperty("faultSourceActivityName", IsKey=true, DefaultValue="*"), StringValidator(MinLength=1)]
        public string FaultSourceActivityName
        {
            get
            {
                return (string) base["faultSourceActivityName"];
            }
            set
            {
                base["faultSourceActivityName"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("faultSourceActivityName", typeof(string), "*", null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("faultHandlerActivityName", typeof(string), "*", null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

