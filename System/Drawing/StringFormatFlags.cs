namespace System.Drawing
{
    using System;

    [Flags]
    public enum StringFormatFlags
    {
        DirectionRightToLeft = 1,
        DirectionVertical = 2,
        DisplayFormatControl = 0x20,
        FitBlackBox = 4,
        LineLimit = 0x2000,
        MeasureTrailingSpaces = 0x800,
        NoClip = 0x4000,
        NoFontFallback = 0x400,
        NoWrap = 0x1000
    }
}

