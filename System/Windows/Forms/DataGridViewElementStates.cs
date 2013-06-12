namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [Flags, ComVisible(true)]
    public enum DataGridViewElementStates
    {
        Displayed = 1,
        Frozen = 2,
        None = 0,
        ReadOnly = 4,
        Resizable = 8,
        ResizableSet = 0x10,
        Selected = 0x20,
        Visible = 0x40
    }
}

