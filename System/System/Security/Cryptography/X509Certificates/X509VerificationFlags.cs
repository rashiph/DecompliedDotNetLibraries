namespace System.Security.Cryptography.X509Certificates
{
    using System;

    [Flags]
    public enum X509VerificationFlags
    {
        AllFlags = 0xfff,
        AllowUnknownCertificateAuthority = 0x10,
        IgnoreCertificateAuthorityRevocationUnknown = 0x400,
        IgnoreCtlNotTimeValid = 2,
        IgnoreCtlSignerRevocationUnknown = 0x200,
        IgnoreEndRevocationUnknown = 0x100,
        IgnoreInvalidBasicConstraints = 8,
        IgnoreInvalidName = 0x40,
        IgnoreInvalidPolicy = 0x80,
        IgnoreNotTimeNested = 4,
        IgnoreNotTimeValid = 1,
        IgnoreRootRevocationUnknown = 0x800,
        IgnoreWrongUsage = 0x20,
        NoFlag = 0
    }
}

