namespace System.Workflow.Runtime.DebugEngine
{
    using System;

    public interface IWorkflowDebuggerService
    {
        void NotifyHandlerInvoked();
        void NotifyHandlerInvoking(Delegate delegateHandler);
    }
}

