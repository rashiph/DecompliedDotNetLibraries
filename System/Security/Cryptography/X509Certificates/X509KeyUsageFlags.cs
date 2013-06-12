namespace System.Security.Cryptography.X509Certificates
{
    using System;

    [Flags]
    public enum X509KeyUsageFlags
    {
        CrlSign = 2,
        DataEncipherment = 0x10,
        DecipherOnly = 0x8000,
        DigitalSignature = 0x80,
        EncipherOnly = 1,
        KeyAgreement = 8,
        KeyCertSign = 4,
        KeyEncipherment = 0x20,
        None = 0,
        NonRepudiation = 0x40
    }
}

