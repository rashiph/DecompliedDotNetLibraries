namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    public class TrackingDataItemValue
    {
        private string _id;
        private string _name;
        private string _value;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TrackingDataItemValue()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TrackingDataItemValue(string qualifiedName, string fieldName, string dataValue)
        {
            this._name = fieldName;
            this._value = dataValue;
            this._id = qualifiedName;
        }

        public string DataValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._value;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._value = value;
            }
        }

        public string FieldName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._name = value;
            }
        }

        public string QualifiedName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._id;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._id = value;
            }
        }
    }
}

