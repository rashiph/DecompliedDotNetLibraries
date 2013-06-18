namespace System.Drawing
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public enum CopyPixelOperation
    {
        Blackness = 0x42,
        CaptureBlt = 0x40000000,
        DestinationInvert = 0x550009,
        MergeCopy = 0xc000ca,
        MergePaint = 0xbb0226,
        NoMirrorBitmap = -2147483648,
        NotSourceCopy = 0x330008,
        NotSourceErase = 0x1100a6,
        PatCopy = 0xf00021,
        PatInvert = 0x5a0049,
        PatPaint = 0xfb0a09,
        SourceAnd = 0x8800c6,
        SourceCopy = 0xcc0020,
        SourceErase = 0x440328,
        SourceInvert = 0x660046,
        SourcePaint = 0xee0086,
        Whiteness = 0xff0062
    }
}

