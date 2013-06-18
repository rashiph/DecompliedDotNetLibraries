namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(ActivityStateQueryElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="activityStateQuery")]
    public class ActivityStateQueryElementCollection : TrackingConfigurationCollection<ActivityStateQueryElement>
    {
        protected override string ElementName
        {
            get
            {
                return "activityStateQuery";
            }
        }
    }
}

