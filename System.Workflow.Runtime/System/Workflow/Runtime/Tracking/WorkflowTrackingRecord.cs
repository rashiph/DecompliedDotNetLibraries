namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public class WorkflowTrackingRecord : TrackingRecord
    {
        private TrackingAnnotationCollection _annotations;
        private System.EventArgs _args;
        private System.Workflow.Runtime.Tracking.TrackingWorkflowEvent _event;
        private DateTime _eventDateTime;
        private int _eventOrder;

        public WorkflowTrackingRecord()
        {
            this._eventDateTime = DateTime.MinValue;
            this._eventOrder = -1;
            this._annotations = new TrackingAnnotationCollection();
        }

        public WorkflowTrackingRecord(System.Workflow.Runtime.Tracking.TrackingWorkflowEvent trackingWorkflowEvent, DateTime eventDateTime, int eventOrder, System.EventArgs eventArgs)
        {
            this._eventDateTime = DateTime.MinValue;
            this._eventOrder = -1;
            this._annotations = new TrackingAnnotationCollection();
            this._event = trackingWorkflowEvent;
            this._eventDateTime = eventDateTime;
            this._eventOrder = eventOrder;
            this._args = eventArgs;
        }

        public override TrackingAnnotationCollection Annotations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._annotations;
            }
        }

        public override System.EventArgs EventArgs
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._args;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._args = value;
            }
        }

        public override DateTime EventDateTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._eventDateTime;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._eventDateTime = value;
            }
        }

        public override int EventOrder
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._eventOrder;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._eventOrder = value;
            }
        }

        public System.Workflow.Runtime.Tracking.TrackingWorkflowEvent TrackingWorkflowEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._event;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._event = value;
            }
        }
    }
}

