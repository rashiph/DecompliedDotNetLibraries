namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    [Serializable]
    public class ActivityTrackingCondition : TrackingCondition
    {
        private ComparisonOperator _op;
        private string _property;
        private string _val;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityTrackingCondition()
        {
        }

        public ActivityTrackingCondition(string member, string value)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            this._property = member;
            this.SetValue(value);
        }

        internal override bool Match(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            object property = PropertyHelper.GetProperty(this._property, obj);
            if (this._op == ComparisonOperator.Equals)
            {
                if (property == null)
                {
                    return (null == this._val);
                }
                return (0 == string.Compare(property.ToString(), this._val, StringComparison.Ordinal));
            }
            if (property == null)
            {
                return (null != this._val);
            }
            return (0 != string.Compare(property.ToString(), this._val, StringComparison.Ordinal));
        }

        private void SetValue(string value)
        {
            this._val = value;
        }

        public override string Member
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._property;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._property = value;
            }
        }

        public override ComparisonOperator Operator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._op;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._op = value;
            }
        }

        public override string Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._val;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.SetValue(value);
            }
        }
    }
}

