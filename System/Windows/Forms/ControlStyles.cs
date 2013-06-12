namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    [Flags]
    public enum ControlStyles
    {
        AllPaintingInWmPaint = 0x2000,
        CacheText = 0x4000,
        ContainerControl = 1,
        [EditorBrowsable(EditorBrowsableState.Never)]
        DoubleBuffer = 0x10000,
        EnableNotifyMessage = 0x8000,
        FixedHeight = 0x40,
        FixedWidth = 0x20,
        Opaque = 4,
        OptimizedDoubleBuffer = 0x20000,
        ResizeRedraw = 0x10,
        Selectable = 0x200,
        StandardClick = 0x100,
        StandardDoubleClick = 0x1000,
        SupportsTransparentBackColor = 0x800,
        UserMouse = 0x400,
        UserPaint = 2,
        UseTextForAccessibility = 0x40000
    }
}

