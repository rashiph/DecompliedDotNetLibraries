namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;

    [Editor("System.Drawing.Design.ContentAlignmentEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public enum ContentAlignment
    {
        BottomCenter = 0x200,
        BottomLeft = 0x100,
        BottomRight = 0x400,
        MiddleCenter = 0x20,
        MiddleLeft = 0x10,
        MiddleRight = 0x40,
        TopCenter = 2,
        TopLeft = 1,
        TopRight = 4
    }
}

