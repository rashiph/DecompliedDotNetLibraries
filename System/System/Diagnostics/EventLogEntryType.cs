namespace System.Diagnostics
{
    using System;

    public enum EventLogEntryType
    {
        Error = 1,
        FailureAudit = 0x10,
        Information = 4,
        SuccessAudit = 8,
        Warning = 2
    }
}

