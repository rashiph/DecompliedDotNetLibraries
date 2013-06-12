namespace System.Threading
{
    using System;

    [Flags]
    internal enum SynchronizationContextProperties
    {
        None,
        RequireWaitNotification
    }
}

