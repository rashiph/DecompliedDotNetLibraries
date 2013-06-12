namespace System.IO.Compression
{
    using System;

    internal static class GZipConstants
    {
        internal const int CompressionLevel_10 = 10;
        internal const int CompressionLevel_3 = 3;
        internal const byte Deflate = 8;
        internal const long FileLengthModulo = 0x100000000L;
        internal const byte ID1 = 0x1f;
        internal const byte ID2 = 0x8b;
        internal const byte Xfl_FastestAlgorithm = 4;
        internal const int Xfl_HeaderPos = 8;
        internal const byte Xfl_MaxCompressionSlowestAlgorithm = 2;
    }
}

