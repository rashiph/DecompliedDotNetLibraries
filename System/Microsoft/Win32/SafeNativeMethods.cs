namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal static class SafeNativeMethods
    {
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;
        public const int MB_RIGHT = 0x80000;
        public const int MB_RTLREADING = 0x100000;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool CloseHandle(HandleRef handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeWaitHandle CreateSemaphore(Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES lpSecurityAttributes, int initialCount, int maximumCount, string name);
        [DllImport("perfcounter.dll", CharSet=CharSet.Auto)]
        public static extern int FormatFromRawValue(uint dwCounterType, uint dwFormat, ref long pTimeBase, Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER pRawValue1, Microsoft.Win32.NativeMethods.PDH_RAW_COUNTER pRawValue2, Microsoft.Win32.NativeMethods.PDH_FMT_COUNTERVALUE pFmtValue);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int FormatMessage(int dwFlags, HandleRef lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int FormatMessage(int dwFlags, SafeHandle lpSource, uint dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool FreeLibrary(HandleRef hModule);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetComputerName(StringBuilder lpBuffer, int[] nSize);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetStockObject(int nIndex);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetTextMetrics(IntPtr hDC, [In, Out] Microsoft.Win32.NativeMethods.TEXTMETRIC tm);
        public static int InterlockedCompareExchange(IntPtr pDestination, int exchange, int compare)
        {
            return Interlocked.CompareExchange(ref (int) ref pDestination.ToPointer(), exchange, compare);
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool IsWow64Process(Microsoft.Win32.SafeHandles.SafeProcessHandle hProcess, ref bool Wow64Process);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern IntPtr LoadLibrary(string libFilename);
        [DllImport("user32.dll", CharSet=CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, int type);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeWaitHandle OpenSemaphore(int desiredAccess, bool inheritHandle, string name);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern void OutputDebugString(string message);
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long value);
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long value);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool ReleaseSemaphore(SafeWaitHandle handle, int releaseCount, out int previousCount);

        [StructLayout(LayoutKind.Sequential)]
        internal class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public int dwProcessId;
            public int dwThreadId;
        }
    }
}

