namespace System.Net
{
    using System;

    internal enum SecurityStatus
    {
        AlgorithmMismatch = -2146893007,
        BadBinding = -2146892986,
        BufferNotEnough = -2146893023,
        CannotInstall = -2146893049,
        CannotPack = -2146893047,
        CertExpired = -2146893016,
        CertUnknown = -2146893017,
        CompAndContinue = 0x90314,
        CompleteNeeded = 0x90313,
        ContextExpired = 0x90317,
        ContinueNeeded = 0x90312,
        CredentialsNeeded = 0x90320,
        IllegalMessage = -2146893018,
        IncompleteCredentials = -2146893024,
        IncompleteMessage = -2146893032,
        InternalError = -2146893052,
        InvalidHandle = -2146893055,
        InvalidToken = -2146893048,
        LogonDenied = -2146893044,
        MessageAltered = -2146893041,
        NoAuthenticatingAuthority = -2146893039,
        NoCredentials = -2146893042,
        NoImpersonation = -2146893045,
        NotOwner = -2146893050,
        OK = 0,
        OutOfMemory = -2146893056,
        OutOfSequence = -2146893040,
        PackageNotFound = -2146893051,
        QopNotSupported = -2146893046,
        Renegotiate = 0x90321,
        SecurityQosFailed = -2146893006,
        SmartcardLogonRequired = -2146892994,
        TargetUnknown = -2146893053,
        TimeSkew = -2146893020,
        UnknownCredentials = -2146893043,
        Unsupported = -2146893054,
        UnsupportedPreauth = -2146892989,
        UntrustedRoot = -2146893019,
        WrongPrincipal = -2146893022
    }
}

