namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    [Flags]
    public enum SyncFromAllServersOptions
    {
        AbortIfServerUnavailable = 1,
        CheckServerAlivenessOnly = 8,
        CrossSite = 0x40,
        None = 0,
        PushChangeOutward = 0x20,
        SkipInitialCheck = 0x10,
        SyncAdjacentServerOnly = 2
    }
}

