namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class CancelRequestedRecord : TrackingRecord
    {
        private CancelRequestedRecord(CancelRequestedRecord record) : base(record)
        {
            this.Activity = record.Activity;
            this.Child = record.Child;
        }

        internal CancelRequestedRecord(Guid instanceId, System.Activities.ActivityInstance instance, System.Activities.ActivityInstance child) : base(instanceId)
        {
            if (instance != null)
            {
                this.Activity = new ActivityInfo(instance);
            }
            this.Child = new ActivityInfo(child);
        }

        public CancelRequestedRecord(Guid instanceId, long recordNumber, ActivityInfo activity, ActivityInfo child) : base(instanceId, recordNumber)
        {
            if (child == null)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("child");
            }
            this.Activity = activity;
            this.Child = child;
        }

        protected internal override TrackingRecord Clone()
        {
            return new CancelRequestedRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "CancelRequestedRecord {{ {0}, Activity {{ {1} }}, ChildActivity {{ {2} }} }}", new object[] { base.ToString(), (this.Activity != null) ? this.Activity.ToString() : "<null>", this.Child.ToString() });
        }

        [DataMember]
        public ActivityInfo Activity { get; private set; }

        [DataMember]
        public ActivityInfo Child { get; private set; }
    }
}

