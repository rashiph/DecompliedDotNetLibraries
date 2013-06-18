namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(ProfileWorkflowElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="workflow")]
    public class ProfileWorkflowElementCollection : TrackingConfigurationCollection<ProfileWorkflowElement>
    {
        protected override string ElementName
        {
            get
            {
                return "workflow";
            }
        }
    }
}

