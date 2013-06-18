namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(VariableElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="variable")]
    public class VariableElementCollection : TrackingConfigurationCollection<VariableElement>
    {
        protected override string ElementName
        {
            get
            {
                return "variable";
            }
        }
    }
}

