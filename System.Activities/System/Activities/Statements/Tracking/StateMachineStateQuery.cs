namespace System.Activities.Statements.Tracking
{
    using System;
    using System.Activities.Tracking;

    public sealed class StateMachineStateQuery : CustomTrackingQuery
    {
        public StateMachineStateQuery()
        {
            base.Name = StateMachineStateRecord.StateMachineStateRecordName;
        }

        public string Name
        {
            get
            {
                return base.Name;
            }
        }
    }
}

