namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;

    internal static class UnsafeNativeMethods
    {
        internal static readonly int GWL_EXSTYLE = -20;
        internal const int S_FALSE = 1;
        internal const int S_OK = 0;
        internal static readonly int WS_EX_LAYOUTRTL = 0x400000;

        [DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }
}

