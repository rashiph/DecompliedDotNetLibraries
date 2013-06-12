namespace System.Net
{
    using System;

    [Flags]
    public enum SecurityProtocolType
    {
        Ssl3 = 0x30,
        Tls = 0xc0
    }
}

