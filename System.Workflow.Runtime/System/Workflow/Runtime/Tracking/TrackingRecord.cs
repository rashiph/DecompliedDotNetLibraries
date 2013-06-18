namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public abstract class TrackingRecord
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TrackingRecord()
        {
        }

        public abstract TrackingAnnotationCollection Annotations { get; }

        public abstract System.EventArgs EventArgs { get; set; }

        public abstract DateTime EventDateTime { get; set; }

        public abstract int EventOrder { get; set; }
    }
}

