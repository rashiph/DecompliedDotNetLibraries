namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(StateElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="state")]
    public sealed class StateElementCollection : TrackingConfigurationCollection<StateElement>
    {
        protected override string ElementName
        {
            get
            {
                return "state";
            }
        }
    }
}

