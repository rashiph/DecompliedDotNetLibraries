namespace System.Net
{
    using System;

    internal interface IReadChunkBytes
    {
        int NextByte { get; set; }
    }
}

