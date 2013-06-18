namespace System.Web.Configuration
{
    using System;

    internal enum RpcAuthent
    {
        DcePrivate = 1,
        DcePublic = 2,
        DecPublic = 4,
        Default = -1,
        Digest = 0x15,
        DPA = 0x11,
        GssKerberos = 0x10,
        GssNegotiate = 9,
        GssSchannel = 14,
        MQ = 100,
        MSN = 0x12,
        None = 0,
        WinNT = 10
    }
}

