namespace System.Net
{
    using System;

    internal enum HttpWriteMode
    {
        Unknown,
        ContentLength,
        Chunked,
        Buffer,
        None
    }
}

