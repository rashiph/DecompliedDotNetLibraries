namespace System.Activities.Tracking
{
    using System;
    using System.Runtime.CompilerServices;

    public class CustomTrackingQuery : TrackingQuery
    {
        public string ActivityName { get; set; }

        public string Name { get; set; }
    }
}

