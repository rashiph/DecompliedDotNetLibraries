namespace System.Web
{
    using System;

    [Flags]
    public enum RequestNotification
    {
        AcquireRequestState = 0x20,
        AuthenticateRequest = 2,
        AuthorizeRequest = 4,
        BeginRequest = 1,
        EndRequest = 0x800,
        ExecuteRequestHandler = 0x80,
        LogRequest = 0x400,
        MapRequestHandler = 0x10,
        PreExecuteRequestHandler = 0x40,
        ReleaseRequestState = 0x100,
        ResolveRequestCache = 8,
        SendResponse = 0x20000000,
        UpdateRequestCache = 0x200
    }
}

