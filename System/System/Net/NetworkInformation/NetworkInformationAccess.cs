namespace System.Net.NetworkInformation
{
    using System;

    [Flags]
    public enum NetworkInformationAccess
    {
        None = 0,
        Ping = 4,
        Read = 1
    }
}

