namespace System.Workflow.Activities
{
    using System;
    using System.Configuration;
    using System.Workflow.Runtime.Configuration;

    public class ExternalDataExchangeServiceSection : ConfigurationSection
    {
        private const string _services = "Services";

        [ConfigurationProperty("Services", DefaultValue=null)]
        public WorkflowRuntimeServiceElementCollection Services
        {
            get
            {
                return (WorkflowRuntimeServiceElementCollection) base["Services"];
            }
        }
    }
}

