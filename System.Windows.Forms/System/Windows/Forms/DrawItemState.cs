namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum DrawItemState
    {
        Checked = 8,
        ComboBoxEdit = 0x1000,
        Default = 0x20,
        Disabled = 4,
        Focus = 0x10,
        Grayed = 2,
        HotLight = 0x40,
        Inactive = 0x80,
        NoAccelerator = 0x100,
        NoFocusRect = 0x200,
        None = 0,
        Selected = 1
    }
}

