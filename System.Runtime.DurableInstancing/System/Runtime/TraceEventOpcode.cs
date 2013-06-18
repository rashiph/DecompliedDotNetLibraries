namespace System.Runtime
{
    using System;

    internal enum TraceEventOpcode
    {
        Info = 0,
        Receive = 240,
        Reply = 6,
        Resume = 7,
        Send = 9,
        Start = 1,
        Stop = 2,
        Suspend = 8
    }
}

