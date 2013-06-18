namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class WorkflowInstanceSuspendedRecord : WorkflowInstanceRecord
    {
        private WorkflowInstanceSuspendedRecord(WorkflowInstanceSuspendedRecord record) : base(record)
        {
            this.Reason = record.Reason;
        }

        public WorkflowInstanceSuspendedRecord(Guid instanceId, string activityDefinitionId, string reason) : base(instanceId, activityDefinitionId, "Suspended")
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }
            this.Reason = reason;
        }

        public WorkflowInstanceSuspendedRecord(Guid instanceId, long recordNumber, string activityDefinitionId, string reason) : base(instanceId, recordNumber, activityDefinitionId, "Suspended")
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }
            this.Reason = reason;
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceSuspendedRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "WorkflowInstanceSuspendedRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, Reason = {4} }} ", new object[] { base.InstanceId, base.RecordNumber, base.EventTime, base.ActivityDefinitionId, this.Reason });
        }

        [DataMember]
        public string Reason { get; private set; }
    }
}

