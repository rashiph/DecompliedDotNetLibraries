namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public const int STANDARD_RIGHTS_REQUIRED = 0xf0000;
        public const int TOKEN_ADJUST_DEFAULT = 0x80;
        public const int TOKEN_ADJUST_GROUPS = 0x40;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        public const int TOKEN_ADJUST_SESSIONID = 0x100;
        public const int TOKEN_ALL_ACCESS = 0xf00ff;
        public const int TOKEN_ASSIGN_PRIMARY = 1;
        public const int TOKEN_DUPLICATE = 2;
        public const int TOKEN_IMPERSONATE = 4;
        public const int TOKEN_QUERY = 8;
        public const int TOKEN_QUERY_SOURCE = 0x10;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool GetKernelObjectSecurity(IntPtr Handle, SECURITY_INFORMATION RequestedInformation, IntPtr pSecurityDescriptor, uint nLength, out uint lpnLengthNeeded);
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool RevertToSelf();
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool SetKernelObjectSecurity(IntPtr Handle, SECURITY_INFORMATION SecurityInformation, IntPtr SecurityDescriptor);

        public enum EoAuthnCap
        {
            AccessControl = 4,
            AnyAuthority = 0x80,
            AppID = 8,
            AutoImpersonate = 0x400,
            Default = 0x800,
            DisableAAA = 0x1000,
            Dynamic = 0x10,
            DynamicCloaking = 0x40,
            MakeFullSIC = 0x100,
            MutualAuth = 1,
            NoCustomMarshal = 0x2000,
            None = 0,
            RequireFullSIC = 0x200,
            SecureRefs = 2,
            StaticCloaking = 0x20
        }

        [Flags]
        public enum RpcAuthnLevel
        {
            Default,
            None,
            Connect,
            Call,
            Pkt,
            PktIntegrity,
            PktPrivacy
        }

        public enum RpcImpLevel
        {
            Default,
            Anonymous,
            Identify,
            Impersonate,
            Delegate
        }

        [Flags]
        public enum SECURITY_INFORMATION : uint
        {
            DACL_SECURITY_INFORMATION = 4,
            GROUP_SECURITY_INFORMATION = 2,
            OWNER_SECURITY_INFORMATION = 1,
            PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,
            PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
            SACL_SECURITY_INFORMATION = 8,
            UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
            UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000
        }
    }
}

