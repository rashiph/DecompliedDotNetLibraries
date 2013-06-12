namespace System.Security.Cryptography.X509Certificates
{
    using System;

    [Flags]
    public enum X509ChainStatusFlags
    {
        CtlNotSignatureValid = 0x40000,
        CtlNotTimeValid = 0x20000,
        CtlNotValidForUsage = 0x80000,
        Cyclic = 0x80,
        HasExcludedNameConstraint = 0x8000,
        HasNotDefinedNameConstraint = 0x2000,
        HasNotPermittedNameConstraint = 0x4000,
        HasNotSupportedNameConstraint = 0x1000,
        InvalidBasicConstraints = 0x400,
        InvalidExtension = 0x100,
        InvalidNameConstraints = 0x800,
        InvalidPolicyConstraints = 0x200,
        NoError = 0,
        NoIssuanceChainPolicy = 0x2000000,
        NotSignatureValid = 8,
        NotTimeNested = 2,
        NotTimeValid = 1,
        NotValidForUsage = 0x10,
        OfflineRevocation = 0x1000000,
        PartialChain = 0x10000,
        RevocationStatusUnknown = 0x40,
        Revoked = 4,
        UntrustedRoot = 0x20
    }
}

