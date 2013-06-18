namespace System.Activities.Tracking
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class CancelRequestedQuery : TrackingQuery
    {
        public CancelRequestedQuery()
        {
            this.ActivityName = "*";
            this.ChildActivityName = "*";
        }

        public string ActivityName { get; set; }

        public string ChildActivityName { get; set; }
    }
}

