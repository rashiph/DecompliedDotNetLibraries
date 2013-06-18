namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Workflow.ComponentModel;

    internal sealed class WorkflowDebuggerService : IWorkflowDebuggerService
    {
        private IWorkflowCoreRuntime coreRuntime;

        internal WorkflowDebuggerService(IWorkflowCoreRuntime coreRuntime)
        {
            if (coreRuntime == null)
            {
                throw new ArgumentNullException("coreRuntime");
            }
            this.coreRuntime = coreRuntime;
        }

        void IWorkflowDebuggerService.NotifyHandlerInvoked()
        {
            this.coreRuntime.RaiseHandlerInvoked();
        }

        void IWorkflowDebuggerService.NotifyHandlerInvoking(Delegate delegateHandler)
        {
            this.coreRuntime.RaiseHandlerInvoking(delegateHandler);
        }
    }
}

