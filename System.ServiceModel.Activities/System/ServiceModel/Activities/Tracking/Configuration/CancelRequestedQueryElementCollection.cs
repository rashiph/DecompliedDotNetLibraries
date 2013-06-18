namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(CancelRequestedQueryElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="cancelRequestedQuery")]
    public class CancelRequestedQueryElementCollection : TrackingConfigurationCollection<CancelRequestedQueryElement>
    {
        protected override string ElementName
        {
            get
            {
                return "cancelRequestedQuery";
            }
        }
    }
}

