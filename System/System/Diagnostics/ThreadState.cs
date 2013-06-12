namespace System.Diagnostics
{
    using System;

    public enum ThreadState
    {
        Initialized,
        Ready,
        Running,
        Standby,
        Terminated,
        Wait,
        Transition,
        Unknown
    }
}

