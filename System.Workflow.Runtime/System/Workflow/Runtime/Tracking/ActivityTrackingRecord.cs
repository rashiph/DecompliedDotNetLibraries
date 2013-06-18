namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class ActivityTrackingRecord : TrackingRecord
    {
        private Type _activityType;
        private TrackingAnnotationCollection _annotations;
        private System.EventArgs _args;
        private List<TrackingDataItem> _body;
        private Guid _contextGuid;
        private DateTime _eventDateTime;
        private int _eventOrder;
        private Guid _parentContextGuid;
        private string _qualifiedID;
        private ActivityExecutionStatus _status;

        public ActivityTrackingRecord()
        {
            this._body = new List<TrackingDataItem>();
            this._contextGuid = Guid.Empty;
            this._parentContextGuid = Guid.Empty;
            this._eventDateTime = DateTime.MinValue;
            this._eventOrder = -1;
            this._annotations = new TrackingAnnotationCollection();
        }

        public ActivityTrackingRecord(Type activityType, string qualifiedName, Guid contextGuid, Guid parentContextGuid, ActivityExecutionStatus executionStatus, DateTime eventDateTime, int eventOrder, System.EventArgs eventArgs)
        {
            this._body = new List<TrackingDataItem>();
            this._contextGuid = Guid.Empty;
            this._parentContextGuid = Guid.Empty;
            this._eventDateTime = DateTime.MinValue;
            this._eventOrder = -1;
            this._annotations = new TrackingAnnotationCollection();
            this._activityType = activityType;
            this._qualifiedID = qualifiedName;
            this._status = executionStatus;
            this._eventDateTime = eventDateTime;
            this._contextGuid = contextGuid;
            this._parentContextGuid = parentContextGuid;
            this._eventOrder = eventOrder;
            this._args = eventArgs;
        }

        public Type ActivityType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._activityType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._activityType = value;
            }
        }

        public override TrackingAnnotationCollection Annotations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._annotations;
            }
        }

        public IList<TrackingDataItem> Body
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._body;
            }
        }

        public Guid ContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._contextGuid;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._contextGuid = value;
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

        public ActivityExecutionStatus ExecutionStatus
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._status;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._status = value;
            }
        }

        public Guid ParentContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._parentContextGuid;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._parentContextGuid = value;
            }
        }

        public string QualifiedName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._qualifiedID;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._qualifiedID = value;
            }
        }
    }
}

