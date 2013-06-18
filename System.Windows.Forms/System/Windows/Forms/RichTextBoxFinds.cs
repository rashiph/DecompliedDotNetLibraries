namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum RichTextBoxFinds
    {
        MatchCase = 4,
        NoHighlight = 8,
        None = 0,
        Reverse = 0x10,
        WholeWord = 2
    }
}

