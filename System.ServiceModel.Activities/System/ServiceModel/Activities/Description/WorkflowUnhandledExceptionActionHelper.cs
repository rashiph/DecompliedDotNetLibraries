namespace System.ServiceModel.Activities.Description
{
    using System;

    internal static class WorkflowUnhandledExceptionActionHelper
    {
        internal static bool IsDefined(WorkflowUnhandledExceptionAction value)
        {
            if (((value != WorkflowUnhandledExceptionAction.Abandon) && (value != WorkflowUnhandledExceptionAction.Cancel)) && (value != WorkflowUnhandledExceptionAction.Terminate))
            {
                return (value == WorkflowUnhandledExceptionAction.AbandonAndSuspend);
            }
            return true;
        }
    }
}

