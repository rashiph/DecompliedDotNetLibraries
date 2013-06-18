namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class FaultPropagationRecord : TrackingRecord
    {
        private FaultPropagationRecord(FaultPropagationRecord record) : base(record)
        {
            this.FaultSource = record.FaultSource;
            this.FaultHandler = record.FaultHandler;
            this.Fault = record.Fault;
            this.IsFaultSource = record.IsFaultSource;
        }

        internal FaultPropagationRecord(Guid instanceId, System.Activities.ActivityInstance source, System.Activities.ActivityInstance faultHandler, bool isFaultSource, Exception fault) : base(instanceId)
        {
            this.FaultSource = new ActivityInfo(source);
            if (faultHandler != null)
            {
                this.FaultHandler = new ActivityInfo(faultHandler);
            }
            this.IsFaultSource = isFaultSource;
            this.Fault = fault;
            base.Level = TraceLevel.Warning;
        }

        public FaultPropagationRecord(Guid instanceId, long recordNumber, ActivityInfo faultSource, ActivityInfo faultHandler, bool isFaultSource, Exception fault) : base(instanceId, recordNumber)
        {
            if (faultSource == null)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("faultSource");
            }
            this.FaultSource = faultSource;
            this.FaultHandler = faultHandler;
            this.IsFaultSource = isFaultSource;
            this.Fault = fault;
            base.Level = TraceLevel.Warning;
        }

        protected internal override TrackingRecord Clone()
        {
            return new FaultPropagationRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "FaultPropagationRecord {{ {0}, FaultSource {{ {1} }}, FaultHandler {{ {2} }}, IsFaultSource = {3} }}", new object[] { base.ToString(), this.FaultSource.ToString(), (this.FaultHandler != null) ? this.FaultHandler.ToString() : "<null>", this.IsFaultSource });
        }

        [DataMember]
        public Exception Fault { get; private set; }

        [DataMember]
        public ActivityInfo FaultHandler { get; private set; }

        [DataMember]
        public ActivityInfo FaultSource { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        public bool IsFaultSource { get; private set; }
    }
}

