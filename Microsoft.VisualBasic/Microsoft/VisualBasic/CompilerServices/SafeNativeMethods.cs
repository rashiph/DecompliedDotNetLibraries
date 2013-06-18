namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false), SecurityCritical, EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class SafeNativeMethods
    {
        private SafeNativeMethods()
        {
        }

        [DllImport("kernel32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern void GetLocalTime(NativeTypes.SystemTime systime);
        [DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern int GetWindowThreadProcessId(IntPtr hwnd, ref int lpdwProcessId);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern bool IsWindowEnabled(IntPtr hwnd);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern bool IsWindowVisible(IntPtr hwnd);
    }
}

