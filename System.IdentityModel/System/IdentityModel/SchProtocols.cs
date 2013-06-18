namespace System.IdentityModel
{
    using System;

    internal enum SchProtocols
    {
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
        Zero = 0
    }
}

