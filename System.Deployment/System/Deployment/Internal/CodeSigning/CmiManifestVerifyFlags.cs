namespace System.Deployment.Internal.CodeSigning
{
    using System;

    [Flags]
    internal enum CmiManifestVerifyFlags
    {
        LifetimeSigning = 0x10,
        None = 0,
        RevocationCheckEndCertOnly = 2,
        RevocationCheckEntireChain = 4,
        RevocationNoCheck = 1,
        StrongNameOnly = 0x10000,
        TrustMicrosoftRootOnly = 0x20,
        UrlCacheOnlyRetrieval = 8
    }
}

