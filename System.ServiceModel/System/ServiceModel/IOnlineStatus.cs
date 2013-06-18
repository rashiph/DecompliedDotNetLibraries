namespace System.ServiceModel
{
    using System;

    public interface IOnlineStatus
    {
        event EventHandler Offline;

        event EventHandler Online;

        bool IsOnline { get; }
    }
}

