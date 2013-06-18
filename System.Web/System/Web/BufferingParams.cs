namespace System.Web
{
    using System;

    internal static class BufferingParams
    {
        internal const int CHAR_BUFFER_SIZE = 0x400;
        internal static readonly int INTEGRATED_MODE_BUFFER_SIZE = (0x4000 - (4 * IntPtr.Size));
        internal const int MAX_BYTES_TO_COPY = 0x80;
        internal const int MAX_FREE_BYTES_TO_CACHE = 0x1000;
        internal const int MAX_FREE_CHAR_BUFFERS = 0x40;
        internal const int MAX_FREE_OUTPUT_BUFFERS = 0x40;
        internal const int MAX_RESOURCE_BYTES_TO_COPY = 0x1000;
        internal const int OUTPUT_BUFFER_SIZE = 0x7c00;
    }
}

