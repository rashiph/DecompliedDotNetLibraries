namespace System.Security.Authentication
{
    using System;

    [Flags]
    public enum SslProtocols
    {
        Default = 240,
        None = 0,
        Ssl2 = 12,
        Ssl3 = 0x30,
        Tls = 0xc0
    }
}

