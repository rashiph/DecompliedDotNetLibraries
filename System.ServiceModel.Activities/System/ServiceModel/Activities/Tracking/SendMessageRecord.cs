namespace System.ServiceModel.Activities.Tracking
{
    using System;
    using System.Activities.Tracking;

    public class SendMessageRecord : CustomTrackingRecord
    {
        protected SendMessageRecord(SendMessageRecord record) : base(record)
        {
        }

        public SendMessageRecord(string name) : base(name)
        {
        }

        protected override TrackingRecord Clone()
        {
            return new SendMessageRecord(this);
        }

        public Guid E2EActivityId
        {
            get
            {
                return (Guid) base.Data["E2EActivityId"];
            }
            set
            {
                base.Data["E2EActivityId"] = value;
            }
        }
    }
}

