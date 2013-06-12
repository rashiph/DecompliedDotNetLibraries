namespace System.Web
{
    using System;

    internal enum IntegratedTraceType
    {
        TraceWrite,
        TraceWarn,
        DiagCritical,
        DiagError,
        DiagWarning,
        DiagInfo,
        DiagVerbose,
        DiagStart,
        DiagStop,
        DiagSuspend,
        DiagResume,
        DiagTransfer
    }
}

