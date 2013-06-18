namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum RichTextBoxLanguageOptions
    {
        AutoFont = 2,
        AutoFontSizeAdjust = 0x10,
        AutoKeyboard = 1,
        DualFont = 0x80,
        ImeAlwaysSendNotify = 8,
        ImeCancelComplete = 4,
        UIFonts = 0x20
    }
}

