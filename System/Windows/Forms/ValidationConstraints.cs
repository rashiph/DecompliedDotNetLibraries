namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum ValidationConstraints
    {
        Enabled = 2,
        ImmediateChildren = 0x10,
        None = 0,
        Selectable = 1,
        TabStop = 8,
        Visible = 4
    }
}

