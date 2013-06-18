namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum DataGridViewPaintParts
    {
        All = 0x7f,
        Background = 1,
        Border = 2,
        ContentBackground = 4,
        ContentForeground = 8,
        ErrorIcon = 0x10,
        Focus = 0x20,
        None = 0,
        SelectionBackground = 0x40
    }
}

