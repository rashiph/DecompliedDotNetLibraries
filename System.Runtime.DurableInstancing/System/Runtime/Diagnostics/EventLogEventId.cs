namespace System.Runtime.Diagnostics
{
    using System;

    internal enum EventLogEventId : uint
    {
        FailedToInitializeTraceSource = 0xc0010065,
        FailedToSetupTracing = 0xc0010064,
        FailedToTraceEvent = 0xc0010068,
        FailedToTraceEventWithException = 0xc0010069,
        FailFast = 0xc0010066,
        FailFastException = 0xc0010067
    }
}

