namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class WorkflowInstanceTerminatedRecord : WorkflowInstanceRecord
    {
        private WorkflowInstanceTerminatedRecord(WorkflowInstanceTerminatedRecord record) : base(record)
        {
            this.Reason = record.Reason;
        }

        public WorkflowInstanceTerminatedRecord(Guid instanceId, string activityDefinitionId, string reason) : base(instanceId, activityDefinitionId, "Terminated")
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }
            this.Reason = reason;
            base.Level = TraceLevel.Error;
        }

        public WorkflowInstanceTerminatedRecord(Guid instanceId, long recordNumber, string activityDefinitionId, string reason) : base(instanceId, recordNumber, activityDefinitionId, "Terminated")
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }
            this.Reason = reason;
            base.Level = TraceLevel.Error;
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceTerminatedRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "WorkflowInstanceTerminatedRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, Reason = {4} }} ", new object[] { base.InstanceId, base.RecordNumber, base.EventTime, base.ActivityDefinitionId, this.Reason });
        }

        [DataMember]
        public string Reason { get; private set; }
    }
}

