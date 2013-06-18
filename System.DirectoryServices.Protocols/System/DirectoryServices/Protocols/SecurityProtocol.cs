namespace System.DirectoryServices.Protocols
{
    using System;

    public enum SecurityProtocol
    {
        Pct1Client = 2,
        Pct1Server = 1,
        Ssl2Client = 8,
        Ssl2Server = 4,
        Ssl3Client = 0x20,
        Ssl3Server = 0x10,
        Tls1Client = 0x80,
        Tls1Server = 0x40
    }
}

