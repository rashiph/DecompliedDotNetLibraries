namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;

    [Flags, Editor("System.Windows.Forms.Design.AnchorEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public enum AnchorStyles
    {
        Bottom = 2,
        Left = 4,
        None = 0,
        Right = 8,
        Top = 1
    }
}

