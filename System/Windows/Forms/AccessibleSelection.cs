namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum AccessibleSelection
    {
        AddSelection = 8,
        ExtendSelection = 4,
        None = 0,
        RemoveSelection = 0x10,
        TakeFocus = 1,
        TakeSelection = 2
    }
}

