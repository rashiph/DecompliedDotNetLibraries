namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal sealed class NativeMethods
    {
        internal const int DNS_ERROR_RCODE_NAME_ERROR = 0x232b;
        internal const int DnsQueryBypassCache = 8;
        internal const int DnsSrvData = 0x21;
        internal const int DS_CANONICAL_NAME = 7;
        internal const int DS_FQDN_1779_NAME = 1;
        internal const int DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING = 6;
        internal const int DS_NAME_FLAG_SYNTACTICAL_ONLY = 1;
        internal const int DS_NAME_NO_ERROR = 0;
        internal const int DsDomainControllerInfoLevel2 = 2;
        internal const int DsDomainControllerInfoLevel3 = 3;
        internal const int DsNameNoError = 0;
        internal const int ERROR_FILE_MARK_DETECTED = 0x44d;
        internal const int ERROR_INVALID_DOMAIN_NAME_FORMAT = 0x4bc;
        internal const int ERROR_INVALID_FLAGS = 0x3ec;
        internal const int ERROR_NO_MORE_ITEMS = 0x103;
        internal const int ERROR_NO_SUCH_DOMAIN = 0x54b;
        internal const int ERROR_NO_SUCH_LOGON_SESSION = 0x520;
        internal const int ERROR_NOT_ENOUGH_MEMORY = 8;
        internal const int NegGetCallerName = 1;
        internal const int STATUS_QUOTA_EXCEEDED = -1073741756;
        internal const int VER_PLATFORM_WIN32_NT = 2;

        private NativeMethods()
        {
        }

        [DllImport("Kernel32.dll", EntryPoint="CompareStringW", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int CompareString([In] uint locale, [In] uint dwCmpFlags, [In] IntPtr lpString1, [In] int cchCount1, [In] IntPtr lpString2, [In] int cchCount2);
        [DllImport("Dnsapi.dll", EntryPoint="DnsQuery_W", CharSet=CharSet.Unicode)]
        internal static extern int DnsQuery([In] string recordName, [In] short recordType, [In] int options, [In] IntPtr servers, out IntPtr dnsResultList, [Out] IntPtr reserved);
        [DllImport("Dnsapi.dll", CharSet=CharSet.Unicode)]
        internal static extern void DnsRecordListFree([In] IntPtr dnsResultList, [In] bool dnsFreeType);
        [DllImport("Netapi32.dll", EntryPoint="DsGetDcCloseW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        internal static extern void DsGetDcClose([In] IntPtr getDcContextHandle);
        [DllImport("Netapi32.dll", EntryPoint="DsGetDcNameW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        internal static extern int DsGetDcName([In] string computerName, [In] string domainName, [In] IntPtr domainGuid, [In] string siteName, [In] int flags, out IntPtr domainControllerInfo);
        [DllImport("Netapi32.dll", EntryPoint="DsGetDcNextW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        internal static extern int DsGetDcNext([In] IntPtr getDcContextHandle, [In, Out] ref IntPtr sockAddressCount, out IntPtr sockAdresses, out IntPtr dnsHostName);
        [DllImport("Netapi32.dll", EntryPoint="DsGetDcOpenW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        internal static extern int DsGetDcOpen([In] string dnsName, [In] int optionFlags, [In] string siteName, [In] IntPtr domainGuid, [In] string dnsForestName, [In] int dcFlags, out IntPtr retGetDcContext);
        [DllImport("Kernel32.dll")]
        internal static extern int GetLastError();
        [DllImport("Kernel32.dll", EntryPoint="GetVersionExW", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool GetVersionEx([In, Out] OSVersionInfoEx ver);
        [DllImport("Secur32.dll")]
        internal static extern int LsaCallAuthenticationPackage([In] LsaLogonProcessSafeHandle lsaHandle, [In] int authenticationPackage, [In] NegotiateCallerNameRequest protocolSubmitBuffer, [In] int submitBufferLength, out IntPtr protocolReturnBuffer, out int returnBufferLength, out int protocolStatus);
        [DllImport("Secur32.dll")]
        internal static extern int LsaConnectUntrusted(out LsaLogonProcessSafeHandle lsaHandle);
        [DllImport("Secur32.dll")]
        internal static extern int LsaDeregisterLogonProcess([In] IntPtr lsaHandle);
        [DllImport("Secur32.dll")]
        internal static extern uint LsaFreeReturnBuffer([In] IntPtr buffer);
        [DllImport("Netapi32.dll")]
        internal static extern int NetApiBufferFree([In] IntPtr buffer);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int DsBindWithCred([MarshalAs(UnmanagedType.LPWStr)] string domainController, [MarshalAs(UnmanagedType.LPWStr)] string dnsDomainName, [In] IntPtr authIdentity, out IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int DsCrackNames([In] IntPtr hDS, [In] int flags, [In] int formatOffered, [In] int formatDesired, [In] int nameCount, [In] IntPtr names, out IntPtr results);

        [SuppressUnmanagedCodeSecurity]
        internal delegate void DsFreeDomainControllerInfo([In] int infoLevel, [In] int dcInfoListCount, [In] IntPtr dcInfoList);

        [SuppressUnmanagedCodeSecurity]
        internal delegate void DsFreePasswordCredentials([In] IntPtr authIdentity);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int DsGetDomainControllerInfo([In] IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string domainName, [In] int infoLevel, out int dcCount, out IntPtr dcInfo);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int DsListRoles([In] IntPtr dsHandle, out IntPtr roles);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int DsListSites([In] IntPtr dsHandle, out IntPtr sites);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int DsMakePasswordCredentials([MarshalAs(UnmanagedType.LPWStr)] string user, [MarshalAs(UnmanagedType.LPWStr)] string domain, [MarshalAs(UnmanagedType.LPWStr)] string password, out IntPtr authIdentity);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int DsUnBind([In] ref IntPtr handle);
    }
}

