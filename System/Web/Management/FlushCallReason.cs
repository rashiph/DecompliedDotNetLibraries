namespace System.Web.Management
{
    using System;

    internal enum FlushCallReason
    {
        UrgentFlushThresholdExceeded,
        Timer,
        StaticFlush
    }
}

