namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal sealed class SafeNativeMethods
    {
        private SafeNativeMethods()
        {
        }

        [DllImport("kernel32.dll")]
        internal static extern int GetCurrentProcessId();
        [DllImport("kernel32.dll")]
        internal static extern int GetCurrentThreadId();
        [DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceCounter([In, Out] ref long lpPerformanceCount);
        [DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceFrequency([In, Out] ref long lpFrequency);
    }
}

