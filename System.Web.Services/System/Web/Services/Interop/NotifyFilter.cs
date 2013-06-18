namespace System.Web.Services.Interop
{
    using System;

    internal enum NotifyFilter
    {
        All = -1,
        AllSync = 15,
        None = 0,
        OnSyncCallEnter = 2,
        OnSyncCallExit = 4,
        OnSyncCallOut = 1,
        OnSyncCallReturn = 8
    }
}

