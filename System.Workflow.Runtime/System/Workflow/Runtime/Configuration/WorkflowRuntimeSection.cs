namespace System.Workflow.Runtime.Configuration
{
    using System;
    using System.Configuration;

    public class WorkflowRuntimeSection : ConfigurationSection
    {
        private const string _definitionCacheCapacity = "WorkflowDefinitionCacheCapacity";
        private const string _enablePerfCounters = "EnablePerformanceCounters";
        private const string _name = "Name";
        private const string _services = "Services";
        private const string _validateOnCreate = "ValidateOnCreate";
        private const string commonParametersSectionName = "CommonParameters";
        internal const string DefaultSectionName = "WorkflowRuntime";

        [ConfigurationProperty("CommonParameters", DefaultValue=null)]
        public NameValueConfigurationCollection CommonParameters
        {
            get
            {
                return (NameValueConfigurationCollection) base["CommonParameters"];
            }
        }

        [ConfigurationProperty("EnablePerformanceCounters", DefaultValue=true)]
        public bool EnablePerformanceCounters
        {
            get
            {
                return (bool) base["EnablePerformanceCounters"];
            }
            set
            {
                base["EnablePerformanceCounters"] = value;
            }
        }

        [ConfigurationProperty("Name", DefaultValue="")]
        public string Name
        {
            get
            {
                return (string) base["Name"];
            }
            set
            {
                base["Name"] = value;
            }
        }

        [ConfigurationProperty("Services", DefaultValue=null)]
        public WorkflowRuntimeServiceElementCollection Services
        {
            get
            {
                return (WorkflowRuntimeServiceElementCollection) base["Services"];
            }
        }

        [ConfigurationProperty("ValidateOnCreate", DefaultValue=true)]
        public bool ValidateOnCreate
        {
            get
            {
                return (bool) base["ValidateOnCreate"];
            }
            set
            {
                base["ValidateOnCreate"] = value;
            }
        }

        [ConfigurationProperty("WorkflowDefinitionCacheCapacity", DefaultValue=0)]
        public int WorkflowDefinitionCacheCapacity
        {
            get
            {
                return (int) base["WorkflowDefinitionCacheCapacity"];
            }
            set
            {
                base["WorkflowDefinitionCacheCapacity"] = value;
            }
        }
    }
}

