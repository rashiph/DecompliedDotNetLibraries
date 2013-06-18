namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(ActivityScheduledQueryElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="activityScheduledQuery")]
    public class ActivityScheduledQueryElementCollection : TrackingConfigurationCollection<ActivityScheduledQueryElement>
    {
        protected override string ElementName
        {
            get
            {
                return "activityScheduledQuery";
            }
        }
    }
}

