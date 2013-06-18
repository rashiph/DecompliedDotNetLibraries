namespace System.Windows.Forms.VisualStyles
{
    using System;

    [Flags]
    public enum HitTestOptions
    {
        BackgroundSegment = 0,
        Caption = 4,
        FixedBorder = 2,
        ResizingBorder = 240,
        ResizingBorderBottom = 0x80,
        ResizingBorderLeft = 0x10,
        ResizingBorderRight = 0x40,
        ResizingBorderTop = 0x20,
        SizingTemplate = 0x100,
        SystemSizingMargins = 0x200
    }
}

