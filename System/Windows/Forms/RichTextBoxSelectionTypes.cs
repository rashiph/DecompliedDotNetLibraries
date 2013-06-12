namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum RichTextBoxSelectionTypes
    {
        Empty = 0,
        MultiChar = 4,
        MultiObject = 8,
        Object = 2,
        Text = 1
    }
}

