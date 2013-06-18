namespace System.ServiceProcess
{
    using System;

    public enum PowerBroadcastStatus
    {
        BatteryLow = 9,
        OemEvent = 11,
        PowerStatusChange = 10,
        QuerySuspend = 0,
        QuerySuspendFailed = 2,
        ResumeAutomatic = 0x12,
        ResumeCritical = 6,
        ResumeSuspend = 7,
        Suspend = 4
    }
}

