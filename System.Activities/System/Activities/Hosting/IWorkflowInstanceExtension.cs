namespace System.Activities.Hosting
{
    using System;
    using System.Collections.Generic;

    public interface IWorkflowInstanceExtension
    {
        IEnumerable<object> GetAdditionalExtensions();
        void SetInstance(WorkflowInstanceProxy instance);
    }
}

