namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(ArgumentElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="argument")]
    public class ArgumentElementCollection : TrackingConfigurationCollection<ArgumentElement>
    {
        protected override string ElementName
        {
            get
            {
                return "argument";
            }
        }
    }
}

