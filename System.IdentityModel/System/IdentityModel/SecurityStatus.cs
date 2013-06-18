namespace System.IdentityModel
{
    using System;

    internal enum SecurityStatus
    {
        BufferNotEnough = -2146893023,
        CannotInstall = -2146893049,
        CompAndContinue = 0x90314,
        CompleteNeeded = 0x90313,
        ContextExpired = 0x90317,
        ContinueNeeded = 0x90312,
        CredentialsNeeded = 0x90320,
        IncompleteCred = -2146893024,
        IncompleteMessage = -2146893032,
        InternalError = -2146893052,
        InvalidHandle = -2146893055,
        InvalidToken = -2146893048,
        LogonDenied = -2146893044,
        MessageAltered = -2146893041,
        NoCredentials = -2146893042,
        NotOwner = -2146893050,
        OK = 0,
        OutOfMemory = -2146893056,
        PackageNotFound = -2146893051,
        Renegotiate = 0x90321,
        TargetUnknown = -2146893053,
        UnknownCertificate = -2146893017,
        UnknownCredential = -2146893043,
        Unsupported = -2146893054,
        UntrustedRoot = -2146893019,
        WrongPrincipal = -2146893022
    }
}

