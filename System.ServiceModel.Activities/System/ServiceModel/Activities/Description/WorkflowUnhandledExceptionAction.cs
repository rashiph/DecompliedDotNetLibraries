namespace System.ServiceModel.Activities.Description
{
    using System;

    public enum WorkflowUnhandledExceptionAction
    {
        Abandon,
        Cancel,
        Terminate,
        AbandonAndSuspend
    }
}

