namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public sealed class WorkflowTrackingLocation
    {
        private IList<TrackingWorkflowEvent> _events;

        public WorkflowTrackingLocation()
        {
            this._events = new List<TrackingWorkflowEvent>();
        }

        public WorkflowTrackingLocation(IList<TrackingWorkflowEvent> events)
        {
            this._events = new List<TrackingWorkflowEvent>();
            this._events = events;
        }

        internal bool Match(TrackingWorkflowEvent status)
        {
            return this._events.Contains(status);
        }

        public IList<TrackingWorkflowEvent> Events
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._events;
            }
        }
    }
}

