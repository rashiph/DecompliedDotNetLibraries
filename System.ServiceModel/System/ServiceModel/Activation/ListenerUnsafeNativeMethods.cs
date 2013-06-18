namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.ComIntegration;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class ListenerUnsafeNativeMethods
    {
        private const string ADVAPI32 = "advapi32.dll";
        internal const int DACL_SECURITY_INFORMATION = 4;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        internal const int ERROR_SERVICE_ALREADY_RUNNING = 0x420;
        private const string KERNEL32 = "kernel32.dll";
        internal const int OWNER_SECURITY_INFORMATION = 1;
        internal const int PROCESS_DUP_HANDLE = 0x40;
        internal const int PROCESS_QUERY_INFORMATION = 0x400;
        internal const int READ_CONTROL = 0x20000;
        internal const int SC_MANAGER_CONNECT = 1;
        internal const int SC_STATUS_PROCESS_INFO = 0;
        internal const int SERVICE_QUERY_CONFIG = 1;
        internal const int SERVICE_QUERY_STATUS = 4;
        internal const int SERVICE_RUNNING = 4;
        internal const int SERVICE_START = 0x10;
        internal const int SERVICE_START_PENDING = 2;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        internal const int TOKEN_QUERY = 8;
        internal const int WRITE_DAC = 0x40000;

        [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe bool AdjustTokenPrivileges(SafeCloseHandle tokenHandle, bool disableAllPrivileges, TOKEN_PRIVILEGES* newState, int bufferLength, IntPtr previousState, IntPtr returnLength);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool CloseServiceHandle(IntPtr handle);
        [DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern void DebugBreak();
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool GetKernelObjectSecurity(SafeCloseHandle handle, int securityInformation, [Out] byte[] pSecurityDescriptor, int nLength, out int lpnLengthNeeded);
        [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool GetTokenInformation(SafeCloseHandle tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, [Out] byte[] pTokenInformation, int tokenInformationLength, out int returnLength);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool IsDebuggerPresent();
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool LookupAccountName(string systemName, string accountName, byte[] sid, ref uint cbSid, StringBuilder referencedDomainName, ref uint cchReferencedDomainName, out short peUse);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern unsafe bool LookupPrivilegeValue(IntPtr lpSystemName, string lpName, LUID* lpLuid);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern SafeCloseHandle OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool OpenProcessToken(SafeCloseHandle processHandle, int desiredAccess, out SafeCloseHandle tokenHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern SafeServiceHandle OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern SafeServiceHandle OpenService(SafeServiceHandle hSCManager, string lpServiceName, int dwDesiredAccess);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool QueryServiceConfig(SafeServiceHandle hService, [Out] byte[] pServiceConfig, int cbBufSize, out int pcbBytesNeeded);
        [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool QueryServiceStatus(SafeServiceHandle hService, out SERVICE_STATUS_PROCESS status);
        [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool QueryServiceStatusEx(SafeServiceHandle hService, int InfoLevel, [Out] byte[] pBuffer, int cbBufSize, out int pcbBytesNeeded);
        [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool SetKernelObjectSecurity(SafeCloseHandle handle, int securityInformation, [In] byte[] pSecurityDescriptor);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool StartService(SafeServiceHandle hSCManager, int dwNumServiceArgs, string[] lpServiceArgVectors);

        [ComImport, ComConversionLoss, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E")]
        internal interface ICorRuntimeHost
        {
            void Void0();
            void Void1();
            void Void2();
            void Void3();
            void Void4();
            void Void5();
            void Void6();
            void Void7();
            void Void8();
            void Void9();
            void GetDefaultDomain([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct QUERY_SERVICE_CONFIG
        {
            internal int dwServiceType;
            internal int dwStartType;
            internal int dwErrorControl;
            internal string lpBinaryPathName;
            internal string lpLoadOrderGroup;
            internal int dwTagId;
            internal string lpDependencies;
            internal string lpServiceStartName;
            internal string lpDisplayName;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SERVICE_STATUS_PROCESS
        {
            internal int dwServiceType;
            internal int dwCurrentState;
            internal int dwControlsAccepted;
            internal int dwWin32ExitCode;
            internal int dwServiceSpecificExitCode;
            internal int dwCheckPoint;
            internal int dwWaitHint;
            internal int dwProcessId;
            internal int dwServiceFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SID_AND_ATTRIBUTES
        {
            internal IntPtr Sid;
            internal ListenerUnsafeNativeMethods.SidAttribute Attributes;
        }

        [Flags]
        internal enum SidAttribute : uint
        {
            SE_GROUP_ENABLED = 4,
            SE_GROUP_ENABLED_BY_DEFAULT = 2,
            SE_GROUP_LOGON_ID = 0xc0000000,
            SE_GROUP_MANDATORY = 1,
            SE_GROUP_OWNER = 8,
            SE_GROUP_RESOURCE = 0x20000000,
            SE_GROUP_USE_FOR_DENY_ONLY = 0x10
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_GROUPS
        {
            internal int GroupCount;
            internal IntPtr Groups;
        }

        internal enum TOKEN_INFORMATION_CLASS
        {
            TokenAuditPolicy = 0x10,
            TokenDefaultDacl = 6,
            TokenGroups = 2,
            TokenGroupsAndPrivileges = 13,
            TokenImpersonationLevel = 9,
            TokenOrigin = 0x11,
            TokenOwner = 4,
            TokenPrimaryGroup = 5,
            TokenPrivileges = 3,
            TokenRestrictedSids = 11,
            TokenSandBoxInert = 15,
            TokenSessionId = 12,
            TokenSessionReference = 14,
            TokenSource = 7,
            TokenStatistics = 10,
            TokenType = 8,
            TokenUser = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_PRIVILEGES
        {
            internal int PrivilegeCount;
            internal LUID_AND_ATTRIBUTES Privileges;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_USER
        {
            internal IntPtr User;
        }
    }
}

