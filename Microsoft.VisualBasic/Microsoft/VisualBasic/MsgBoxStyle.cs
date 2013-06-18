namespace Microsoft.VisualBasic
{
    using System;

    [Flags]
    public enum MsgBoxStyle
    {
        AbortRetryIgnore = 2,
        ApplicationModal = 0,
        Critical = 0x10,
        DefaultButton1 = 0,
        DefaultButton2 = 0x100,
        DefaultButton3 = 0x200,
        Exclamation = 0x30,
        Information = 0x40,
        MsgBoxHelp = 0x4000,
        MsgBoxRight = 0x80000,
        MsgBoxRtlReading = 0x100000,
        MsgBoxSetForeground = 0x10000,
        OkCancel = 1,
        OkOnly = 0,
        Question = 0x20,
        RetryCancel = 5,
        SystemModal = 0x1000,
        YesNo = 4,
        YesNoCancel = 3
    }
}

