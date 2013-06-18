namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public class TrackingDataItem
    {
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private object _data;
        private string _fieldName;

        public TrackingAnnotationCollection Annotations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._annotations;
            }
        }

        public object Data
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._data;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._data = value;
            }
        }

        public string FieldName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._fieldName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._fieldName = value;
            }
        }
    }
}

