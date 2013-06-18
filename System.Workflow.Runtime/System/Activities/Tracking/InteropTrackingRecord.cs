namespace System.Activities.Tracking
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Workflow.Runtime.Tracking;

    public class InteropTrackingRecord : CustomTrackingRecord
    {
        protected InteropTrackingRecord(InteropTrackingRecord record) : base(record)
        {
            this.TrackingRecord = record.TrackingRecord;
        }

        public InteropTrackingRecord(string activityDisplayName, System.Workflow.Runtime.Tracking.TrackingRecord v1TrackingRecord) : base(activityDisplayName)
        {
            this.TrackingRecord = v1TrackingRecord;
            base.Data.Add("TrackingRecord", v1TrackingRecord);
        }

        protected override System.Activities.Tracking.TrackingRecord Clone()
        {
            return new InteropTrackingRecord(this);
        }

        public System.Workflow.Runtime.Tracking.TrackingRecord TrackingRecord
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<TrackingRecord>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<TrackingRecord>k__BackingField = value;
            }
        }
    }
}

