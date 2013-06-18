namespace System.Activities.Statements.Tracking
{
    using System;
    using System.Activities.Tracking;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class StateMachineStateRecord : CustomTrackingRecord
    {
        private const string StateKey = "currentstate";
        private const string StateMachineKey = "stateMachine";
        internal static readonly string StateMachineStateRecordName = "System.Activities.Statements.StateMachine";

        public StateMachineStateRecord() : this(StateMachineStateRecordName)
        {
        }

        private StateMachineStateRecord(StateMachineStateRecord record) : base(record)
        {
        }

        internal StateMachineStateRecord(string name) : base(name)
        {
        }

        internal StateMachineStateRecord(string name, TraceLevel level) : base(name, level)
        {
        }

        internal StateMachineStateRecord(Guid instanceId, string name, TraceLevel level) : base(instanceId, name, level)
        {
        }

        protected internal override TrackingRecord Clone()
        {
            return new StateMachineStateRecord(this);
        }

        public string StateMachineName
        {
            get
            {
                if (base.Data.ContainsKey("stateMachine"))
                {
                    return base.Data["stateMachine"].ToString();
                }
                return string.Empty;
            }
            internal set
            {
                base.Data["stateMachine"] = value;
            }
        }

        [DataMember]
        public string StateName
        {
            get
            {
                if (base.Data.ContainsKey("currentstate"))
                {
                    return base.Data["currentstate"].ToString();
                }
                return string.Empty;
            }
            internal set
            {
                base.Data["currentstate"] = value;
            }
        }
    }
}

