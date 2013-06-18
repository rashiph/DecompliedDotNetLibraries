namespace System.ServiceModel
{
    using System;

    [Flags]
    internal enum UnifiedSecurityMode
    {
        Both = 0x10,
        Message = 8,
        None = 1,
        Transport = 4,
        TransportCredentialOnly = 0x40,
        TransportWithMessageCredential = 0x20
    }
}

