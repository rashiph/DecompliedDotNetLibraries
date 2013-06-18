namespace System.ServiceModel.Activation
{
    using System;

    [Flags]
    internal enum HttpAccessSslFlags
    {
        None = 0,
        Ssl = 8,
        Ssl128 = 0x100,
        SslMapCert = 0x80,
        SslNegotiateCert = 0x20,
        SslRequireCert = 0x40
    }
}

