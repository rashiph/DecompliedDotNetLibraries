namespace System.Activities.Tracking
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class FaultPropagationQuery : TrackingQuery
    {
        public FaultPropagationQuery()
        {
            this.FaultSourceActivityName = "*";
            this.FaultHandlerActivityName = "*";
        }

        public string FaultHandlerActivityName { get; set; }

        public string FaultSourceActivityName { get; set; }
    }
}

