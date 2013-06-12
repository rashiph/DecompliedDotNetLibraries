namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public enum X509ContentType
    {
        Authenticode = 6,
        Cert = 1,
        Pfx = 3,
        Pkcs12 = 3,
        Pkcs7 = 5,
        SerializedCert = 2,
        SerializedStore = 4,
        Unknown = 0
    }
}

