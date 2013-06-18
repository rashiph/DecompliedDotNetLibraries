namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public sealed class WorkflowTrackPoint
    {
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private WorkflowTrackingLocation _location = new WorkflowTrackingLocation();

        internal bool IsMatch(TrackingWorkflowEvent status)
        {
            return this._location.Match(status);
        }

        public TrackingAnnotationCollection Annotations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._annotations;
            }
        }

        public WorkflowTrackingLocation MatchingLocation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._location;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._location = value;
            }
        }
    }
}

