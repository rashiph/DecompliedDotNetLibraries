namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.IdentityModel;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Principal;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        internal const string ADVAPI32 = "advapi32.dll";
        internal const string COMSVCS = "comsvcs.dll";
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int ERROR_MORE_DATA = 0xea;
        internal const int ERROR_NOT_SUPPORTED = 50;
        internal const int ERROR_SUCCESS = 0;
        internal const int HWND_BROADCAST = 0xffff;
        internal const string KERNEL32 = "kernel32.dll";
        internal const int KEY_CREATE_LINK = 0x20;
        internal const int KEY_CREATE_SUB_KEY = 4;
        internal const int KEY_ENUMERATE_SUB_KEYS = 8;
        internal const int KEY_NOTIFY = 0x10;
        internal const int KEY_QUERY_VALUE = 1;
        internal const int KEY_READ = 0x20019;
        internal const int KEY_SET_VALUE = 2;
        internal const int KEY_WOW64_32KEY = 0x200;
        internal const int KEY_WOW64_64KEY = 0x100;
        internal const int KEY_WRITE = 0x20006;
        internal const string OLE32 = "ole32.dll";
        internal const string OLEAUT32 = "oleaut32.dll";
        internal const int READ_CONTROL = 0x20000;
        internal const int REG_BINARY = 3;
        internal const int REG_DWORD = 4;
        internal const int REG_DWORD_BIG_ENDIAN = 5;
        internal const int REG_DWORD_LITTLE_ENDIAN = 4;
        internal const int REG_EXPAND_SZ = 2;
        internal const int REG_FULL_RESOURCE_DESCRIPTOR = 9;
        internal const int REG_LINK = 6;
        internal const int REG_MULTI_SZ = 7;
        internal const int REG_NONE = 0;
        internal const int REG_QWORD = 11;
        internal const int REG_RESOURCE_LIST = 8;
        internal const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
        internal const int REG_SZ = 1;
        internal const string SECUR32 = "secur32.dll";
        internal const int STANDARD_RIGHTS_READ = 0x20000;
        internal const int STANDARD_RIGHTS_WRITE = 0x20000;
        internal const int SYNCHRONIZE = 0x100000;
        internal const int WM_SETTINGCHANGE = 0x1a;

        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool AccessCheck([In] byte[] SecurityDescriptor, [In] SafeCloseHandle ClientToken, [In] int DesiredAccess, [In] GENERIC_MAPPING GenericMapping, out PRIVILEGE_SET PrivilegeSet, [In, Out] ref uint PrivilegeSetLength, out uint GrantedAccess, out bool AccessStatus);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("comsvcs.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern object CoCreateActivity([In, MarshalAs(UnmanagedType.IUnknown)] object pIUnknown, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern object CoCreateInstance([In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] CLSCTX dwClsContext, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern object CoGetObjectContext([In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        internal static extern IntPtr CoSwitchCallContext(IntPtr newSecurityObject);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern IStream CreateStreamOnHGlobal([In] SafeHGlobalHandle hGlobal, [In, MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool DuplicateTokenEx([In] SafeCloseHandle ExistingToken, [In] TokenAccessLevels DesiredAccess, [In] IntPtr TokenAttributes, [In] SecurityImpersonationLevel ImpersonationLevel, [In] System.ServiceModel.ComIntegration.TokenType TokenType, out SafeCloseHandle NewToken);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", EntryPoint="OpenProcessToken", SetLastError=true)]
        internal static extern bool GetCurrentProcessToken([In] IntPtr ProcessHandle, [In] TokenAccessLevels DesiredAccess, out SafeCloseHandle TokenHandle);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll")]
        internal static extern int GetCurrentThreadId();
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern SafeHGlobalHandle GetHGlobalFromStream(IStream stream);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool GetTokenInformation([In] SafeCloseHandle TokenHandle, [In] TOKEN_INFORMATION_CLASS TokenInformationClass, [In] SafeHandle TokenInformation, [Out] uint TokenInformationLength, out uint ReturnLength);
        [DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern IntPtr GlobalLock(SafeHGlobalHandle hGlobal);
        [DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern IntPtr GlobalSize(SafeHGlobalHandle hGlobal);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern bool GlobalUnlock(SafeHGlobalHandle hGlobal);
        [DllImport("advapi32.dll", EntryPoint="ImpersonateAnonymousToken", SetLastError=true)]
        internal static extern bool ImpersonateAnonymousUserOnCurrentThread([In] IntPtr CurrentThread);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int LoadRegTypeLib(ref Guid rguid, ushort major, ushort minor, int lcid, [MarshalAs(UnmanagedType.Interface)] out object typeLib);
        [DllImport("advapi32.dll", EntryPoint="OpenThreadToken", SetLastError=true)]
        internal static extern bool OpenCurrentThreadToken([In] IntPtr ThreadHandle, [In] TokenAccessLevels DesiredAccess, [In] bool OpenAsSelf, out SafeCloseHandle TokenHandle);
        [DllImport("advapi32.dll")]
        internal static extern int RegCloseKey(IntPtr handle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int RegDeleteKey(RegistryHandle hKey, string lpValueName);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int RegEnumKey(RegistryHandle hKey, int index, StringBuilder lpName, ref int len);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int RegOpenKeyEx(RegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out RegistryHandle hkResult);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int RegQueryValueEx(RegistryHandle hKey, string lpValueName, int[] lpReserved, ref int lpType, [Out] byte[] lpData, ref int lpcbData);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int RegSetValueEx(RegistryHandle hKey, string lpValueName, int Reserved, int dwType, string val, int cbData);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool RevertToSelf();
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        internal static extern IntPtr SafeArrayAccessData(IntPtr pSafeArray);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int SafeArrayGetDim(IntPtr pSafeArray);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int SafeArrayGetElemsize(IntPtr pSafeArray);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        internal static extern int SafeArrayGetLBound(IntPtr pSafeArray, int cDims);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        internal static extern int SafeArrayGetUBound(IntPtr pSafeArray, int cDims);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        internal static extern void SafeArrayUnaccessData(IntPtr pSafeArray);
        [DllImport("advapi32.dll", EntryPoint="SetThreadToken", SetLastError=true)]
        internal static extern bool SetCurrentThreadToken([In] IntPtr ThreadHandle, [In] SafeCloseHandle TokenHandle);
        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport("secur32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool TranslateName(string input, EXTENDED_NAME_FORMAT inputFormat, EXTENDED_NAME_FORMAT outputFormat, StringBuilder outputString, out uint size);
    }
}

