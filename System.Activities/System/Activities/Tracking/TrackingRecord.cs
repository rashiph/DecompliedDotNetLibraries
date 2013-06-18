namespace System.Activities.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class TrackingRecord
    {
        [DataMember(EmitDefaultValue=false)]
        private IDictionary<string, string> annotations;
        private static ReadOnlyDictionary<string, string> readonlyEmptyAnnotations;

        protected TrackingRecord(TrackingRecord record)
        {
            this.InstanceId = record.InstanceId;
            this.RecordNumber = record.RecordNumber;
            this.EventTime = record.EventTime;
            this.Level = record.Level;
            if (record.HasAnnotations)
            {
                this.annotations = new ReadOnlyDictionary<string, string>(record.annotations);
            }
        }

        protected TrackingRecord(Guid instanceId)
        {
            this.InstanceId = instanceId;
            this.EventTime = DateTime.UtcNow;
            this.Level = TraceLevel.Info;
        }

        protected TrackingRecord(Guid instanceId, long recordNumber) : this(instanceId)
        {
            this.RecordNumber = recordNumber;
        }

        protected internal abstract TrackingRecord Clone();
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "InstanceId = {0}, RecordNumber = {1}, EventTime = {2}", new object[] { this.InstanceId, this.RecordNumber, this.EventTime });
        }

        public IDictionary<string, string> Annotations
        {
            get
            {
                if (this.annotations == null)
                {
                    this.annotations = ReadOnlyEmptyAnnotations;
                }
                return this.annotations;
            }
            internal set
            {
                this.annotations = value;
            }
        }

        [DataMember]
        public DateTime EventTime { get; private set; }

        internal bool HasAnnotations
        {
            get
            {
                return ((this.annotations != null) && (this.annotations.Count > 0));
            }
        }

        [DataMember]
        public Guid InstanceId { get; internal set; }

        [DataMember]
        public TraceLevel Level { get; protected set; }

        private static ReadOnlyDictionary<string, string> ReadOnlyEmptyAnnotations
        {
            get
            {
                if (readonlyEmptyAnnotations == null)
                {
                    readonlyEmptyAnnotations = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(0));
                }
                return readonlyEmptyAnnotations;
            }
        }

        [DataMember]
        public long RecordNumber { get; internal set; }
    }
}

