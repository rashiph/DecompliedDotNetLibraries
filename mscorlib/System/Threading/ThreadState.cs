namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum ThreadState
    {
        Aborted = 0x100,
        AbortRequested = 0x80,
        Background = 4,
        Running = 0,
        Stopped = 0x10,
        StopRequested = 1,
        Suspended = 0x40,
        SuspendRequested = 2,
        Unstarted = 8,
        WaitSleepJoin = 0x20
    }
}

