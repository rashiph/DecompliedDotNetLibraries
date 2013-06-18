namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [ComVisible(false), SuppressUnmanagedCodeSecurity]
    internal class UnsafeNativeMethods
    {
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;

        [DllImport("activeds.dll", CharSet=CharSet.Unicode)]
        public static extern int ADsEncodeBinaryData(byte[] data, int length, ref IntPtr result);
        [DllImport("Kernel32.dll")]
        public static extern int CloseHandle(IntPtr handle);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int ConvertSidToStringSidW(IntPtr pSid, ref IntPtr stringSid);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int ConvertStringSidToSidW(IntPtr stringSid, ref IntPtr pSid);
        [DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
        public static extern int DsEnumerateDomainTrustsW(string serverName, int flags, out IntPtr domains, out int count);
        [DllImport("netapi32.dll", EntryPoint="DsGetSiteNameW", CharSet=CharSet.Unicode)]
        public static extern int DsGetSiteName(string dcName, ref IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        public static extern int FormatMessageW(int dwFlags, int lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, int arguments);
        [DllImport("activeds.dll")]
        public static extern bool FreeADsMem(IntPtr pVoid);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        public static extern uint FreeLibrary(IntPtr libName);
        [DllImport("Kernel32.dll")]
        public static extern int GetCurrentThreadId();
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr GetProcAddress(LoadLibrarySafeHandle hModule, string entryPoint);
        [DllImport("Kernel32.dll")]
        public static extern void GetSystemTimeAsFileTime(IntPtr fileTime);
        [DllImport("netapi32.dll", CharSet=CharSet.Unicode)]
        public static extern int I_NetLogonControl2(string serverName, int FunctionCode, int QueryLevel, IntPtr data, out IntPtr buffer);
        [DllImport("Advapi32.dll", SetLastError=true)]
        public static extern int ImpersonateAnonymousToken(IntPtr token);
        [DllImport("Advapi32.dll", SetLastError=true)]
        public static extern int ImpersonateLoggedOnUser(IntPtr hToken);
        [DllImport("Kernel32.dll", EntryPoint="LoadLibraryW", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern IntPtr LoadLibrary(string name);
        [DllImport("kernel32.dll")]
        public static extern int LocalFree(IntPtr mem);
        [DllImport("Advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int LogonUserW(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        [DllImport("Advapi32.dll")]
        public static extern int LsaClose(IntPtr handle);
        [DllImport("Advapi32.dll")]
        public static extern int LsaCreateTrustedDomainEx(PolicySafeHandle handle, TRUSTED_DOMAIN_INFORMATION_EX domainEx, TRUSTED_DOMAIN_AUTH_INFORMATION authInfo, int classInfo, out IntPtr domainHandle);
        [DllImport("Advapi32.dll")]
        public static extern int LsaDeleteTrustedDomain(PolicySafeHandle handle, IntPtr pSid);
        [DllImport("Advapi32.dll")]
        public static extern int LsaFreeMemory(IntPtr ptr);
        [DllImport("Advapi32.dll")]
        public static extern int LsaNtStatusToWinError(int status);
        [DllImport("Advapi32.dll")]
        public static extern int LsaOpenPolicy(LSA_UNICODE_STRING target, LSA_OBJECT_ATTRIBUTES objectAttributes, int access, out IntPtr handle);
        [DllImport("Advapi32.dll")]
        public static extern int LsaOpenTrustedDomainByName(PolicySafeHandle policyHandle, LSA_UNICODE_STRING trustedDomain, int access, ref IntPtr trustedDomainHandle);
        [DllImport("Advapi32.dll")]
        public static extern int LsaQueryForestTrustInformation(PolicySafeHandle handle, LSA_UNICODE_STRING target, ref IntPtr ForestTrustInfo);
        [DllImport("Advapi32.dll")]
        public static extern int LsaQueryInformationPolicy(PolicySafeHandle handle, int infoClass, out IntPtr buffer);
        [DllImport("Advapi32.dll")]
        public static extern int LsaQueryTrustedDomainInfoByName(PolicySafeHandle handle, LSA_UNICODE_STRING trustedDomain, TRUSTED_INFORMATION_CLASS infoClass, ref IntPtr buffer);
        [DllImport("Advapi32.dll")]
        public static extern int LsaSetForestTrustInformation(PolicySafeHandle handle, LSA_UNICODE_STRING target, IntPtr forestTrustInfo, int checkOnly, out IntPtr collisionInfo);
        [DllImport("Advapi32.dll")]
        public static extern int LsaSetTrustedDomainInfoByName(PolicySafeHandle handle, LSA_UNICODE_STRING trustedDomain, TRUSTED_INFORMATION_CLASS infoClass, IntPtr buffer);
        [DllImport("Netapi32.dll")]
        public static extern int NetApiBufferFree(IntPtr buffer);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern IntPtr OpenThread(uint desiredAccess, bool inheirted, int threadID);
        [DllImport("Advapi32.dll", SetLastError=true)]
        public static extern int RevertToSelf();
        [DllImport("ntdll.dll")]
        public static extern int RtlInitUnicodeString(LSA_UNICODE_STRING result, IntPtr s);

        [SuppressUnmanagedCodeSecurity]
        public delegate void DsFreeNameResultW(IntPtr result);

        [SuppressUnmanagedCodeSecurity]
        public delegate int DsListDomainsInSiteW(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string site, ref IntPtr info);

        [SuppressUnmanagedCodeSecurity]
        public delegate int DsReplicaConsistencyCheck([In] IntPtr handle, int taskID, int flags);

        [SuppressUnmanagedCodeSecurity]
        public delegate int DsReplicaFreeInfo(int type, IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        public delegate int DsReplicaGetInfo2W(IntPtr handle, int type, [MarshalAs(UnmanagedType.LPWStr)] string objectPath, IntPtr sourceGUID, string attributeName, string value, int flag, int context, ref IntPtr info);

        [SuppressUnmanagedCodeSecurity]
        public delegate int DsReplicaGetInfoW(IntPtr handle, int type, [MarshalAs(UnmanagedType.LPWStr)] string objectPath, IntPtr sourceGUID, ref IntPtr info);

        [SuppressUnmanagedCodeSecurity]
        public delegate int DsReplicaSyncAllW(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string partition, int flags, SyncReplicaFromAllServersCallback callback, IntPtr data, ref IntPtr error);

        [SuppressUnmanagedCodeSecurity]
        public delegate int DsReplicaSyncW(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string partition, IntPtr uuid, int option);
    }
}

