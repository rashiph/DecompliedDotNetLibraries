namespace System.Activities.Tracking
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class ActivityScheduledQuery : TrackingQuery
    {
        public ActivityScheduledQuery()
        {
            this.ActivityName = "*";
            this.ChildActivityName = "*";
        }

        public string ActivityName { get; set; }

        public string ChildActivityName { get; set; }
    }
}

