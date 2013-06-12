namespace System.Diagnostics
{
    using System;

    public enum ProcessPriorityClass
    {
        AboveNormal = 0x8000,
        BelowNormal = 0x4000,
        High = 0x80,
        Idle = 0x40,
        Normal = 0x20,
        RealTime = 0x100
    }
}

