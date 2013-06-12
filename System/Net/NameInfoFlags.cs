namespace System.Net
{
    using System;

    [Flags]
    internal enum NameInfoFlags
    {
        NI_DGRAM = 0x10,
        NI_NAMEREQD = 4,
        NI_NOFQDN = 1,
        NI_NUMERICHOST = 2,
        NI_NUMERICSERV = 8
    }
}

