namespace System.Diagnostics
{
    using System;

    [Serializable]
    internal enum LoggingLevels
    {
        ErrorLevel = 50,
        PanicLevel = 100,
        StatusLevel0 = 20,
        StatusLevel1 = 0x15,
        StatusLevel2 = 0x16,
        StatusLevel3 = 0x17,
        StatusLevel4 = 0x18,
        TraceLevel0 = 0,
        TraceLevel1 = 1,
        TraceLevel2 = 2,
        TraceLevel3 = 3,
        TraceLevel4 = 4,
        WarningLevel = 40
    }
}

