namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class WorkflowInstanceUnhandledExceptionRecord : WorkflowInstanceRecord
    {
        private WorkflowInstanceUnhandledExceptionRecord(WorkflowInstanceUnhandledExceptionRecord record) : base(record)
        {
            this.FaultSource = record.FaultSource;
            this.UnhandledException = record.UnhandledException;
        }

        public WorkflowInstanceUnhandledExceptionRecord(Guid instanceId, string activityDefinitionId, ActivityInfo faultSource, Exception exception) : this(instanceId, 0L, activityDefinitionId, faultSource, exception)
        {
        }

        public WorkflowInstanceUnhandledExceptionRecord(Guid instanceId, long recordNumber, string activityDefinitionId, ActivityInfo faultSource, Exception exception) : base(instanceId, recordNumber, activityDefinitionId, "UnhandledException")
        {
            if (string.IsNullOrEmpty(activityDefinitionId))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("activityDefinitionId");
            }
            if (exception == null)
            {
                throw FxTrace.Exception.ArgumentNull("exception");
            }
            if (faultSource == null)
            {
                throw FxTrace.Exception.ArgumentNull("faultSource");
            }
            this.FaultSource = faultSource;
            this.UnhandledException = exception;
            base.Level = TraceLevel.Error;
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceUnhandledExceptionRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "WorkflowInstanceUnhandledExceptionRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, FaultSource {{ {4} }}, UnhandledException = {5} }} ", new object[] { base.InstanceId, base.RecordNumber, base.EventTime, base.ActivityDefinitionId, this.FaultSource.ToString(), this.UnhandledException });
        }

        [DataMember]
        public ActivityInfo FaultSource { get; private set; }

        [DataMember]
        public Exception UnhandledException { get; private set; }
    }
}

