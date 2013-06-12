namespace System.Diagnostics.Eventing
{
    using System;
    using System.Runtime.CompilerServices;

    [FriendAccessAllowed]
    internal enum EventOpcode
    {
        DataCollectionStart = 3,
        DataCollectionStop = 4,
        Extension = 5,
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

