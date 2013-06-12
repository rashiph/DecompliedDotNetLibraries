namespace System.Net
{
    using System;

    internal enum WebExceptionInternalStatus
    {
        RequestFatal,
        ServicePointFatal,
        Recoverable,
        Isolated
    }
}

