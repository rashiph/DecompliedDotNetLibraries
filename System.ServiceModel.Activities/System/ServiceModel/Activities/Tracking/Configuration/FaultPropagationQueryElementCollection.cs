namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(FaultPropagationQueryElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="faultPropagationQuery")]
    public class FaultPropagationQueryElementCollection : TrackingConfigurationCollection<FaultPropagationQueryElement>
    {
        protected override string ElementName
        {
            get
            {
                return "faultPropagationQuery";
            }
        }
    }
}

