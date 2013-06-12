namespace System.Diagnostics.Eventing
{
    using System;
    using System.Runtime.CompilerServices;

    [FriendAccessAllowed]
    internal enum EventLevel
    {
        LogAlways,
        Critical,
        Error,
        Warning,
        Informational,
        Verbose
    }
}

