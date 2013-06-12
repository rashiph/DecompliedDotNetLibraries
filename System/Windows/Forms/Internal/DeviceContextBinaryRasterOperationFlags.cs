namespace System.Windows.Forms.Internal
{
    using System;

    [Flags]
    internal enum DeviceContextBinaryRasterOperationFlags
    {
        Black = 1,
        CopyPen = 13,
        MaskNotPen = 3,
        MaskPen = 9,
        MaskPenNot = 5,
        MergeNotPen = 12,
        MergePen = 15,
        MergePenNot = 14,
        Nop = 11,
        Not = 6,
        NotCopyPen = 4,
        NotMaskPen = 8,
        NotMergePen = 2,
        NotXorPen = 10,
        White = 0x10,
        XorPen = 7
    }
}

