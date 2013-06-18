namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum ButtonState
    {
        All = 0x4700,
        Checked = 0x400,
        Flat = 0x4000,
        Inactive = 0x100,
        Normal = 0,
        Pushed = 0x200
    }
}

