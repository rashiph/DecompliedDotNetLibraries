namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public abstract class TrackingChannel
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TrackingChannel()
        {
        }

        protected internal abstract void InstanceCompletedOrTerminated();
        protected internal abstract void Send(TrackingRecord record);
    }
}

