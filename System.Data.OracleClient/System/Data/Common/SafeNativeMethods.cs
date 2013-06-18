namespace System.Data.Common
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeNativeMethods
    {
        private SafeNativeMethods()
        {
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int GetCurrentProcessId();
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalAlloc(int flags, IntPtr countOfBytes);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalFree(IntPtr handle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int ReleaseSemaphore(IntPtr handle, int releaseCount, IntPtr previousCount);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int WaitForMultipleObjectsEx(uint nCount, IntPtr lpHandles, bool bWaitAll, uint dwMilliseconds, bool bAlertable);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll")]
        internal static extern int WaitForSingleObjectEx(IntPtr lpHandles, uint dwMilliseconds, bool bAlertable);
    }
}

