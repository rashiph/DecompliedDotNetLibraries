namespace System.Net
{
    using System;

    internal enum CertificateEncoding
    {
        AnyAsnEncoding = 0x10001,
        Pkcs7AsnEncoding = 0x10000,
        Pkcs7NdrEncoding = 0x20000,
        X509AsnEncoding = 1,
        X509NdrEncoding = 2,
        Zero = 0
    }
}

