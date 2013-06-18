namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public class WorkflowInstanceRecord : TrackingRecord
    {
        protected WorkflowInstanceRecord(WorkflowInstanceRecord record) : base(record)
        {
            this.ActivityDefinitionId = record.ActivityDefinitionId;
            this.State = record.State;
        }

        public WorkflowInstanceRecord(Guid instanceId, string activityDefinitionId, string state) : base(instanceId)
        {
            if (string.IsNullOrEmpty(activityDefinitionId))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("activityDefinitionId");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("state");
            }
            this.ActivityDefinitionId = activityDefinitionId;
            this.State = state;
        }

        public WorkflowInstanceRecord(Guid instanceId, long recordNumber, string activityDefinitionId, string state) : base(instanceId, recordNumber)
        {
            if (string.IsNullOrEmpty(activityDefinitionId))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("activityDefinitionId");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("state");
            }
            this.ActivityDefinitionId = activityDefinitionId;
            this.State = state;
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "WorkflowInstanceRecord {{ {0}, ActivityDefinitionId = {1}, State = {2} }}", new object[] { base.ToString(), this.ActivityDefinitionId, this.State });
        }

        [DataMember]
        public string ActivityDefinitionId { get; private set; }

        [DataMember]
        public string State { get; private set; }
    }
}

