namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Activities.Tracking;
    using System.Configuration;

    public class WorkflowInstanceQueryElement : TrackingQueryElement
    {
        private ConfigurationPropertyCollection properties;

        protected override TrackingQuery NewTrackingQuery()
        {
            WorkflowInstanceQuery query = new WorkflowInstanceQuery();
            foreach (StateElement element in this.States)
            {
                query.States.Add(element.Name);
            }
            return query;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("states", typeof(StateElementCollection), null, null, null, ConfigurationPropertyOptions.None));
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
    }
}

