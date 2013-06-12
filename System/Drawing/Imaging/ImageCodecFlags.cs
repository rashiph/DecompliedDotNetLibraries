namespace System.Drawing.Imaging
{
    using System;

    [Flags]
    public enum ImageCodecFlags
    {
        BlockingDecode = 0x20,
        Builtin = 0x10000,
        Decoder = 2,
        Encoder = 1,
        SeekableEncode = 0x10,
        SupportBitmap = 4,
        SupportVector = 8,
        System = 0x20000,
        User = 0x40000
    }
}

