namespace System.Security.Cryptography
{
    using System;

    public enum SignatureVerificationResult
    {
        AssemblyIdentityMismatch = 1,
        BadDigest = -2146869232,
        BadSignatureFormat = -2146762749,
        BasicConstraintsNotObserved = -2146869223,
        CertificateExpired = -2146762495,
        CertificateExplicitlyDistrusted = -2146762479,
        CertificateMalformed = -2146762488,
        CertificateNotExplicitlyTrusted = -2146762748,
        CertificateRevoked = -2146762484,
        CertificateUsageNotAllowed = -2146762490,
        ContainingSignatureInvalid = 2,
        CouldNotBuildChain = -2146762486,
        GenericTrustFailure = -2146762485,
        InvalidCertificateName = -2146762476,
        InvalidCertificatePolicy = -2146762477,
        InvalidCertificateRole = -2146762493,
        InvalidCertificateSignature = -2146869244,
        InvalidCertificateUsage = -2146762480,
        InvalidCountersignature = -2146869245,
        InvalidSignerCertificate = -2146869246,
        InvalidTimePeriodNesting = -2146762494,
        InvalidTimestamp = -2146869243,
        IssuerChainingError = -2146762489,
        MissingSignature = -2146762496,
        PathLengthConstraintViolated = -2146762492,
        PublicKeyTokenMismatch = 3,
        PublisherMismatch = 4,
        RevocationCheckFailure = -2146762482,
        SystemError = -2146869247,
        UnknownCriticalExtension = -2146762491,
        UnknownTrustProvider = -2146762751,
        UnknownVerificationAction = -2146762750,
        UntrustedCertificationAuthority = -2146762478,
        UntrustedRootCertificate = -2146762487,
        UntrustedTestRootCertificate = -2146762483,
        Valid = 0
    }
}

