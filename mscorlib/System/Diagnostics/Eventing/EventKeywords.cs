namespace System.Diagnostics.Eventing
{
    using System;
    using System.Runtime.CompilerServices;

    [Flags, FriendAccessAllowed]
    internal enum EventKeywords : long
    {
        AuditFailure = 0x10000000000000L,
        AuditSuccess = 0x20000000000000L,
        CorrelationHint = 0x10000000000000L,
        EventLogClassic = 0x80000000000000L,
        None = 0L,
        Sqm = 0x8000000000000L,
        WdiContext = 0x2000000000000L,
        WdiDiagnostic = 0x4000000000000L
    }
}

