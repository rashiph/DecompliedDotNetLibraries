namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public class CustomTrackingRecord : TrackingRecord
    {
        [DataMember(EmitDefaultValue=false)]
        private IDictionary<string, object> data;

        protected CustomTrackingRecord(CustomTrackingRecord record) : base(record)
        {
            this.Name = record.Name;
            this.Activity = record.Activity;
            if ((record.data != null) && (record.data.Count > 0))
            {
                foreach (KeyValuePair<string, object> pair in record.data)
                {
                    this.Data.Add(pair);
                }
            }
        }

        public CustomTrackingRecord(string name) : this(name, TraceLevel.Info)
        {
        }

        public CustomTrackingRecord(string name, TraceLevel level) : this(Guid.Empty, name, level)
        {
        }

        public CustomTrackingRecord(Guid instanceId, string name, TraceLevel level) : base(instanceId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNull("name");
            }
            this.Name = name;
            base.Level = level;
        }

        protected internal override TrackingRecord Clone()
        {
            return new CustomTrackingRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "CustomTrackingRecord {{ {0}, Name={1}, Activity {{ {2} }}, Level = {3} }}", new object[] { base.ToString(), this.Name, (this.Activity == null) ? "<null>" : this.Activity.ToString(), base.Level });
        }

        [DataMember]
        public ActivityInfo Activity { get; internal set; }

        public IDictionary<string, object> Data
        {
            get
            {
                if (this.data == null)
                {
                    this.data = new Dictionary<string, object>();
                }
                return this.data;
            }
        }

        [DataMember]
        public string Name { get; private set; }
    }
}

