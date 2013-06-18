namespace System.Activities
{
    using System;

    public interface IExecutionProperty
    {
        void CleanupWorkflowThread();
        void SetupWorkflowThread();
    }
}

