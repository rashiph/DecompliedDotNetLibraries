namespace System.IO.Compression
{
    using System;

    internal enum InflaterState
    {
        DecodeTop = 10,
        DecodingUncompressed = 20,
        Done = 0x18,
        HaveDistCode = 13,
        HaveFullLength = 12,
        HaveInitialLength = 11,
        ReadingBFinal = 2,
        ReadingBType = 3,
        ReadingCodeLengthCodes = 7,
        ReadingFooter = 0x16,
        ReadingHeader = 0,
        ReadingNumCodeLengthCodes = 6,
        ReadingNumDistCodes = 5,
        ReadingNumLitCodes = 4,
        ReadingTreeCodesAfter = 9,
        ReadingTreeCodesBefore = 8,
        StartReadingFooter = 0x15,
        UncompressedAligning = 15,
        UncompressedByte1 = 0x10,
        UncompressedByte2 = 0x11,
        UncompressedByte3 = 0x12,
        UncompressedByte4 = 0x13,
        VerifyingFooter = 0x17
    }
}

