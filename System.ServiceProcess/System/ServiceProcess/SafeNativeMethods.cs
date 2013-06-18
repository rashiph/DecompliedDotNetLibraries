namespace System.ServiceProcess
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal static class SafeNativeMethods
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool CloseServiceHandle(IntPtr handle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool GetServiceDisplayName(IntPtr SCMHandle, string shortName, StringBuilder displayName, ref int displayNameLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool GetServiceKeyName(IntPtr SCMHandle, string displayName, StringBuilder shortName, ref int shortNameLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        public static extern int LsaClose(IntPtr objectHandle);
        [DllImport("advapi32.dll")]
        public static extern int LsaFreeMemory(IntPtr ptr);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        public static extern int LsaNtStatusToWinError(int ntStatus);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, int access);
    }
}

