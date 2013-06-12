namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum DataGridViewDataErrorContexts
    {
        ClipboardContent = 0x4000,
        Commit = 0x200,
        CurrentCellChange = 0x1000,
        Display = 2,
        Formatting = 1,
        InitialValueRestoration = 0x400,
        LeaveControl = 0x800,
        Parsing = 0x100,
        PreferredSize = 4,
        RowDeletion = 8,
        Scroll = 0x2000
    }
}

