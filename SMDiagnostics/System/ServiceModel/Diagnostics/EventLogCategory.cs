namespace System.ServiceModel.Diagnostics
{
    using System;

    internal enum EventLogCategory : ushort
    {
        ComPlus = 10,
        FailFast = 6,
        ListenerAdapter = 14,
        MessageAuthentication = 2,
        MessageLogging = 7,
        ObjectAccess = 3,
        PerformanceCounter = 8,
        ServiceAuthorization = 1,
        SharingService = 13,
        StateMachine = 11,
        Tracing = 4,
        WebHost = 5,
        Wmi = 9,
        Wsat = 12
    }
}

