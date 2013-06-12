namespace System.Net
{
    using System;

    internal interface IRequestLifetimeTracker
    {
        void TrackRequestLifetime(long requestStartTimestamp);
    }
}

