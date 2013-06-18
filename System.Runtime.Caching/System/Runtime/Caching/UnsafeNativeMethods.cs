namespace System.Runtime.Caching
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity, SecurityCritical]
    internal static class UnsafeNativeMethods
    {
        private const string ADVAPI32 = "ADVAPI32.DLL";
        private const string KERNEL32 = "KERNEL32.DLL";

        [DllImport("KERNEL32.DLL", CharSet=CharSet.Unicode)]
        internal static extern int GetModuleFileName(IntPtr module, StringBuilder filename, int size);
        [DllImport("KERNEL32.DLL", CharSet=CharSet.Unicode)]
        internal static extern int GlobalMemoryStatusEx(ref MEMORYSTATUSEX memoryStatusEx);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ADVAPI32.DLL")]
        internal static extern int RegCloseKey(IntPtr hKey);
    }
}

