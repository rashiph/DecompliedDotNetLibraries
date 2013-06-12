namespace System.Net.Mail
{
    using System;

    internal enum ServerState
    {
        Continuing = 7,
        Paused = 6,
        Pausing = 5,
        Started = 2,
        Starting = 1,
        Stopped = 4,
        Stopping = 3
    }
}

