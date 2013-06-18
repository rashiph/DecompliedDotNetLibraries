namespace System.ServiceModel.Activities.Tracking
{
    using System;
    using System.Activities.Tracking;

    public class ReceiveMessageRecord : CustomTrackingRecord
    {
        protected ReceiveMessageRecord(ReceiveMessageRecord record) : base(record)
        {
        }

        public ReceiveMessageRecord(string name) : base(name)
        {
        }

        protected override TrackingRecord Clone()
        {
            return new ReceiveMessageRecord(this);
        }

        public Guid E2EActivityId
        {
            get
            {
                return (Guid) base.Data["E2EActivityId"];
            }
            internal set
            {
                base.Data["E2EActivityId"] = value;
            }
        }

        public Guid MessageId
        {
            get
            {
                return (Guid) base.Data["MessageId"];
            }
        }
    }
}

