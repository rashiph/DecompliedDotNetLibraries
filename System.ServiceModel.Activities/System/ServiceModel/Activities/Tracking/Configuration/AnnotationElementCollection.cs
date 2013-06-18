namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(AnnotationElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="annotation")]
    public class AnnotationElementCollection : TrackingConfigurationCollection<AnnotationElement>
    {
        protected override string ElementName
        {
            get
            {
                return "annotation";
            }
        }
    }
}

