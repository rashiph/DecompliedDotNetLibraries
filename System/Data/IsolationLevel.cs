namespace System.Data
{
    using System;

    public enum IsolationLevel
    {
        Chaos = 0x10,
        ReadCommitted = 0x1000,
        ReadUncommitted = 0x100,
        RepeatableRead = 0x10000,
        Serializable = 0x100000,
        Snapshot = 0x1000000,
        Unspecified = -1
    }
}

