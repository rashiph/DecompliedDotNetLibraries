namespace System.IO
{
    using System;

    [Flags]
    public enum WatcherChangeTypes
    {
        All = 15,
        Changed = 4,
        Created = 1,
        Deleted = 2,
        Renamed = 8
    }
}

