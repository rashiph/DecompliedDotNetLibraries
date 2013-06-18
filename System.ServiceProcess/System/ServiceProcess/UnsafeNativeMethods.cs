namespace System.ServiceProcess
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(false), SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern unsafe bool ControlService(IntPtr serviceHandle, int control, System.ServiceProcess.NativeMethods.SERVICE_STATUS* pStatus);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool EnumDependentServices(IntPtr serviceHandle, int serviceState, IntPtr bufferOfENUM_SERVICE_STATUS, int bufSize, ref int bytesNeeded, ref int numEnumerated);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool EnumServicesStatus(IntPtr databaseHandle, int serviceType, int serviceState, IntPtr status, int size, out int bytesNeeded, out int servicesReturned, ref int resumeHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool EnumServicesStatusEx(IntPtr databaseHandle, int infolevel, int serviceType, int serviceState, IntPtr status, int size, out int bytesNeeded, out int servicesReturned, ref int resumeHandle, string group);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern IntPtr OpenService(IntPtr databaseHandle, string serviceName, int access);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool QueryServiceConfig(IntPtr serviceHandle, IntPtr query_service_config_ptr, int bufferSize, out int bytesNeeded);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern unsafe bool QueryServiceStatus(IntPtr serviceHandle, System.ServiceProcess.NativeMethods.SERVICE_STATUS* pStatus);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool StartService(IntPtr serviceHandle, int argNum, IntPtr argPtrs);
    }
}

