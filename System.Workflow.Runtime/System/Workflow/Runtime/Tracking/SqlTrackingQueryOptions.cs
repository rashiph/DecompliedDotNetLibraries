namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    public class SqlTrackingQueryOptions
    {
        private List<TrackingDataItemValue> _dataItems = new List<TrackingDataItemValue>();
        private DateTime _max = DateTime.MaxValue;
        private DateTime _min = DateTime.MinValue;
        private System.Workflow.Runtime.WorkflowStatus? _status = null;
        private Type _type;

        public void Clear()
        {
            this._min = DateTime.MinValue;
            this._max = DateTime.MaxValue;
            this._status = null;
            this._type = null;
            this._dataItems = new List<TrackingDataItemValue>();
        }

        public DateTime StatusMaxDateTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._max;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._max = value;
            }
        }

        public DateTime StatusMinDateTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._min;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._min = value;
            }
        }

        public IList<TrackingDataItemValue> TrackingDataItems
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._dataItems;
            }
        }

        public System.Workflow.Runtime.WorkflowStatus? WorkflowStatus
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

        public Type WorkflowType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._type;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._type = value;
            }
        }
    }
}

