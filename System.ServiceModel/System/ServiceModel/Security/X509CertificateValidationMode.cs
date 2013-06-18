namespace System.ServiceModel.Security
{
    using System;

    public enum X509CertificateValidationMode
    {
        None,
        PeerTrust,
        ChainTrust,
        PeerOrChainTrust,
        Custom
    }
}

