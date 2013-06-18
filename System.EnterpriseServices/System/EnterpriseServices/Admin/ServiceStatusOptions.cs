namespace System.EnterpriseServices.Admin
{
    using System;

    [Serializable]
    internal enum ServiceStatusOptions
    {
        Stopped,
        StartPending,
        StopPending,
        Running,
        ContinuePending,
        PausePending,
        Paused,
        UnknownState
    }
}

