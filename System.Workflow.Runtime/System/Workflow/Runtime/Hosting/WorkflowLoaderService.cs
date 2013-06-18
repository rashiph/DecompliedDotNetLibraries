namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Xml;

    public abstract class WorkflowLoaderService : WorkflowRuntimeService
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowLoaderService()
        {
        }

        protected internal abstract Activity CreateInstance(Type workflowType);
        protected internal abstract Activity CreateInstance(XmlReader workflowDefinitionReader, XmlReader rulesReader);
    }
}

