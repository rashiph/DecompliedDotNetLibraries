namespace System.Net
{
    using System;

    internal enum SchProtocols
    {
        ClientMask = -2147483478,
        Pct = 3,
        PctClient = 2,
        PctServer = 1,
        ServerMask = 0x40000055,
        Ssl2 = 12,
        Ssl2Client = 8,
        Ssl2Server = 4,
        Ssl3 = 0x30,
        Ssl3Client = 0x20,
        Ssl3Server = 0x10,
        Ssl3Tls = 240,
        Tls = 0xc0,
        TlsClient = 0x80,
        TlsServer = 0x40,
        UniClient = -2147483648,
        Unified = -1073741824,
        UniServer = 0x40000000,
        Zero = 0
    }
}

