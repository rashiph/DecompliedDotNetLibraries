namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;

    [Serializable, Flags]
    public enum LogRecordFlags
    {
        ForgetTarget = 1,
        ReplayInProgress = 0x40,
        WrittenDuringAbort = 8,
        WrittenDuringCommit = 4,
        WrittenDuringPrepare = 2,
        WrittenDuringReplay = 0x20,
        WrittenDurringRecovery = 0x10
    }
}

