namespace System.Security.Cryptography.Pkcs
{
    using System;

    internal enum RecipientSubType
    {
        Unknown,
        Pkcs7KeyTransport,
        CmsKeyTransport,
        CertIdKeyAgreement,
        PublicKeyAgreement
    }
}

