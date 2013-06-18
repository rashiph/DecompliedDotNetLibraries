namespace System.ServiceModel.Activation
{
    using System;

    internal enum ListenerExceptionStatus
    {
        Success,
        PathTooLong,
        RegistrationQuotaExceeded,
        ProtocolUnsupported,
        ConflictingRegistration,
        FailedToListen,
        VersionUnsupported,
        InvalidArgument
    }
}

