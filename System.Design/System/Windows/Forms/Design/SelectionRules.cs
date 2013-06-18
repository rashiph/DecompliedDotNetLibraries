namespace System.Windows.Forms.Design
{
    using System;

    [Flags]
    public enum SelectionRules
    {
        AllSizeable = 15,
        BottomSizeable = 2,
        LeftSizeable = 4,
        Locked = -2147483648,
        Moveable = 0x10000000,
        None = 0,
        RightSizeable = 8,
        TopSizeable = 1,
        Visible = 0x40000000
    }
}

