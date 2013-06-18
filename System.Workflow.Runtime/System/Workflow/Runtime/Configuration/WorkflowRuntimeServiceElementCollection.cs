namespace System.Workflow.Runtime.Configuration
{
    using System;
    using System.Configuration;

    public class WorkflowRuntimeServiceElementCollection : ConfigurationElementCollection
    {
        public void Add(WorkflowRuntimeServiceElement serviceSettings)
        {
            if (serviceSettings == null)
            {
                throw new ArgumentNullException("serviceSettings");
            }
            base.BaseAdd(serviceSettings);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WorkflowRuntimeServiceElement();
        }

        protected override object GetElementKey(ConfigurationElement settings)
        {
            return ((WorkflowRuntimeServiceElement) settings).Type;
        }
    }
}

