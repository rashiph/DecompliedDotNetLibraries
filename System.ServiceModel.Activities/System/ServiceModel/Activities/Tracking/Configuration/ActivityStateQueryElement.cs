namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Activities.Tracking;
    using System.Configuration;

    public class ActivityStateQueryElement : TrackingQueryElement
    {
        private ConfigurationPropertyCollection properties;

        protected override TrackingQuery NewTrackingQuery()
        {
            ActivityStateQuery query = new ActivityStateQuery {
                ActivityName = this.ActivityName
            };
            foreach (StateElement element in this.States)
            {
                query.States.Add(element.Name);
            }
            foreach (VariableElement element2 in this.Variables)
            {
                query.Variables.Add(element2.Name);
            }
            foreach (ArgumentElement element3 in this.Arguments)
            {
                query.Arguments.Add(element3.Name);
            }
            return query;
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

        [ConfigurationProperty("arguments")]
        public ArgumentElementCollection Arguments
        {
            get
            {
                return (ArgumentElementCollection) base["arguments"];
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
                    properties.Add(new ConfigurationProperty("states", typeof(StateElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("variables", typeof(VariableElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("arguments", typeof(ArgumentElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("states")]
        public StateElementCollection States
        {
            get
            {
                return (StateElementCollection) base["states"];
            }
        }

        [ConfigurationProperty("variables")]
        public VariableElementCollection Variables
        {
            get
            {
                return (VariableElementCollection) base["variables"];
            }
        }
    }
}

