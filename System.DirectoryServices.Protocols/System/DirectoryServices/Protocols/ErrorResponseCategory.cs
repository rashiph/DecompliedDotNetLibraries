namespace System.DirectoryServices.Protocols
{
    using System;

    public enum ErrorResponseCategory
    {
        NotAttempted,
        CouldNotConnect,
        ConnectionClosed,
        MalformedRequest,
        GatewayInternalError,
        AuthenticationFailed,
        UnresolvableUri,
        Other
    }
}

