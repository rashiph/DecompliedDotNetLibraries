namespace System.Workflow.Runtime
{
    using System;
    using System.Diagnostics;

    internal class WorkflowTraceTransfer : IDisposable
    {
        private Guid oldGuid = Trace.CorrelationManager.ActivityId;
        private bool transferBackAtClose;

        public WorkflowTraceTransfer(Guid instanceId)
        {
            if (!this.oldGuid.Equals(instanceId))
            {
                WorkflowTrace.Runtime.TraceTransfer(0, null, instanceId);
                Trace.CorrelationManager.ActivityId = instanceId;
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Start, 0, "Workflow Trace");
                this.transferBackAtClose = true;
            }
        }

        public void Dispose()
        {
            if (this.transferBackAtClose)
            {
                WorkflowTrace.Runtime.TraceTransfer(0, null, this.oldGuid);
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Stop, 0, "Workflow Trace");
                Trace.CorrelationManager.ActivityId = this.oldGuid;
            }
        }
    }
}

