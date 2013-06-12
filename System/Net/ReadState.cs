namespace System.Net
{
    using System;

    internal enum ReadState
    {
        Start,
        StatusLine,
        Headers,
        Data
    }
}

