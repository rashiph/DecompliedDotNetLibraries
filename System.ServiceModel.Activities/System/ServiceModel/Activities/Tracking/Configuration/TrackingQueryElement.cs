namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Configuration;

    public abstract class TrackingQueryElement : TrackingConfigurationElement
    {
        private Guid? elementKey;
        private ConfigurationPropertyCollection properties;

        protected TrackingQueryElement()
        {
        }

        internal TrackingQuery CreateTrackingQuery()
        {
            TrackingQuery trackingQuery = this.NewTrackingQuery();
            this.UpdateTrackingQuery(trackingQuery);
            return trackingQuery;
        }

        protected abstract TrackingQuery NewTrackingQuery();
        protected virtual void UpdateTrackingQuery(TrackingQuery trackingQuery)
        {
            foreach (AnnotationElement element in this.Annotations)
            {
                trackingQuery.QueryAnnotations.Add(new KeyValuePair<string, string>(element.Name, element.Value));
            }
        }

        [ConfigurationProperty("annotations")]
        public AnnotationElementCollection Annotations
        {
            get
            {
                return (AnnotationElementCollection) base["annotations"];
            }
        }

        public override object ElementKey
        {
            get
            {
                if (!this.elementKey.HasValue)
                {
                    this.elementKey = new Guid?(Guid.NewGuid());
                }
                return this.elementKey;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("annotations", typeof(AnnotationElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

