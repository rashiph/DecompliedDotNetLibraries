namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Flags]
    public enum TreeViewHitTestLocations
    {
        AboveClientArea = 0x100,
        BelowClientArea = 0x200,
        Image = 2,
        Indent = 8,
        Label = 4,
        LeftOfClientArea = 0x800,
        None = 1,
        PlusMinus = 0x10,
        RightOfClientArea = 0x400,
        RightOfLabel = 0x20,
        StateImage = 0x40
    }
}

