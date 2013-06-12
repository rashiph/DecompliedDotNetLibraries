namespace System.Net
{
    using System;

    internal enum HttpProcessingResult
    {
        Continue,
        ReadWait,
        WriteWait
    }
}

