namespace System.Net
{
    using System;

    internal enum BoundaryType
    {
        Chunked = 1,
        ContentLength = 0,
        Invalid = 5,
        Multipart = 3,
        None = 4
    }
}

