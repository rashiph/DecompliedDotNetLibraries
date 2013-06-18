namespace System.Windows.Forms.Design
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    internal interface ISupportInSituService
    {
        IntPtr GetEditWindow();
        void HandleKeyChar();

        bool IgnoreMessages { get; }
    }
}

