namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(WorkflowInstanceQueryElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="workflowInstanceQuery")]
    public sealed class WorkflowInstanceQueryElementCollection : TrackingConfigurationCollection<WorkflowInstanceQueryElement>
    {
        protected override string ElementName
        {
            get
            {
                return "workflowInstanceQuery";
            }
        }
    }
}

