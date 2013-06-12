namespace System.Net
{
    using System;

    [Flags]
    internal enum ThreadKinds
    {
        Async = 8,
        CompletionPort = 0x20,
        Finalization = 0x80,
        Other = 0x100,
        OwnerMask = 3,
        SafeSources = 0x160,
        SourceMask = 0x1f0,
        Sync = 4,
        SyncMask = 12,
        System = 2,
        ThreadPool = 0x60,
        Timer = 0x10,
        Unknown = 0,
        User = 1,
        Worker = 0x40
    }
}

