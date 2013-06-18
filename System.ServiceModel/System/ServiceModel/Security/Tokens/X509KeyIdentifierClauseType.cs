namespace System.ServiceModel.Security.Tokens
{
    using System;

    public enum X509KeyIdentifierClauseType
    {
        Any,
        Thumbprint,
        IssuerSerial,
        SubjectKeyIdentifier,
        RawDataKeyIdentifier
    }
}

