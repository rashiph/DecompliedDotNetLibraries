namespace System.Windows.Forms
{
    using System;

    public enum CloseReason
    {
        None,
        WindowsShutDown,
        MdiFormClosing,
        UserClosing,
        TaskManagerClosing,
        FormOwnerClosing,
        ApplicationExitCall
    }
}

