namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum DragDropEffects
    {
        All = -2147483645,
        Copy = 1,
        Link = 4,
        Move = 2,
        None = 0,
        Scroll = -2147483648
    }
}

