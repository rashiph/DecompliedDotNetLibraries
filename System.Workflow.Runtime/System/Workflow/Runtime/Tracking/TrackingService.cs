namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.Runtime.Hosting;

    public abstract class TrackingService : WorkflowRuntimeService
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TrackingService()
        {
        }

        protected internal abstract TrackingProfile GetProfile(Guid workflowInstanceId);
        protected internal abstract TrackingProfile GetProfile(Type workflowType, Version profileVersionId);
        protected internal abstract TrackingChannel GetTrackingChannel(TrackingParameters parameters);
        protected internal abstract bool TryGetProfile(Type workflowType, out TrackingProfile profile);
        protected internal abstract bool TryReloadProfile(Type workflowType, Guid workflowInstanceId, out TrackingProfile profile);
    }
}

