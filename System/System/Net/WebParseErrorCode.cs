namespace System.Net
{
    using System;

    internal enum WebParseErrorCode
    {
        Generic,
        InvalidHeaderName,
        InvalidContentLength,
        IncompleteHeaderLine,
        CrLfError,
        InvalidChunkFormat,
        UnexpectedServerResponse
    }
}

