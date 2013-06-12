namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum UICues
    {
        Changed = 12,
        ChangeFocus = 4,
        ChangeKeyboard = 8,
        None = 0,
        ShowFocus = 1,
        ShowKeyboard = 2,
        Shown = 3
    }
}

