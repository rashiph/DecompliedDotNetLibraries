namespace System.Net.NetworkInformation
{
    using System;

    [Flags]
    internal enum StartIPOptions
    {
        None,
        StartIPv4,
        StartIPv6,
        Both
    }
}

