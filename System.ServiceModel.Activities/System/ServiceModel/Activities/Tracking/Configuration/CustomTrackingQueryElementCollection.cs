namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(CustomTrackingQueryElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="customTrackingQuery")]
    public class CustomTrackingQueryElementCollection : TrackingConfigurationCollection<CustomTrackingQueryElement>
    {
        protected override string ElementName
        {
            get
            {
                return "customTrackingQuery";
            }
        }
    }
}

