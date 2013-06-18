namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum ListViewHitTestLocations
    {
        AboveClientArea = 0x100,
        BelowClientArea = 0x10,
        Image = 2,
        Label = 4,
        LeftOfClientArea = 0x40,
        None = 1,
        RightOfClientArea = 0x20,
        StateImage = 0x200
    }
}

