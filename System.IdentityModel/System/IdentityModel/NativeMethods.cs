namespace System.IdentityModel
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        private const string ADVAPI32 = "advapi32.dll";
        internal const int ALG_CLASS_DATA_ENCRYPT = 0x6000;
        internal const int ALG_TYPE_BLOCK = 0x600;
        internal const int CALG_AES_128 = 0x660e;
        internal const int CALG_AES_192 = 0x660f;
        internal const int CALG_AES_256 = 0x6610;
        private const string CREDUI = "credui.dll";
        internal const uint CRYPT_DELETEKEYSET = 0x10;
        internal const uint CRYPT_VERIFYCONTEXT = 0xf0000000;
        internal const byte CUR_BLOB_VERSION = 2;
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_BAD_LENGTH = 0x18;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        internal const uint KERB_CERTIFICATE_S4U_LOGON_FLAG_CHECK_DUPLICATES = 1;
        internal const uint KERB_CERTIFICATE_S4U_LOGON_FLAG_CHECK_LOGONHOURS = 2;
        private const string KERNEL32 = "kernel32.dll";
        internal const int KP_IV = 1;
        internal static byte[] LsaKerberosName = new byte[] { 0x4b, 0x65, 0x72, 0x62, 0x65, 0x72, 0x6f, 0x73 };
        internal static byte[] LsaSourceName = new byte[] { 0x57, 0x43, 70 };
        internal const byte PLAINTEXTKEYBLOB = 8;
        internal const int PROV_RSA_AES = 0x18;
        internal const uint SE_GROUP_ENABLED = 4;
        internal const uint SE_GROUP_LOGON_ID = 0xc0000000;
        internal const uint SE_GROUP_USE_FOR_DENY_ONLY = 0x10;
        private const string SECUR32 = "secur32.dll";
        internal const uint STATUS_ACCESS_DENIED = 0xc0000022;
        internal const uint STATUS_ACCOUNT_RESTRICTION = 0xc000006e;
        internal const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;
        internal const uint STATUS_NO_MEMORY = 0xc0000017;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool AdjustTokenPrivileges([In] SafeCloseHandle tokenHandle, [In] bool disableAllPrivileges, [In] ref TOKEN_PRIVILEGE newState, [In] uint bufferLength, out TOKEN_PRIVILEGE previousState, out uint returnLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool AllocateLocallyUniqueId(out LUID Luid);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptAcquireContextW(out SafeProvHandle phProv, [In] string pszContainer, [In] string pszProvider, [In] uint dwProvType, [In] uint dwFlags);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe bool CryptDecrypt([In] SafeKeyHandle phKey, [In] IntPtr hHash, [In] bool final, [In] uint dwFlags, [In] void* pbData, [In, Out] ref int dwDataLen);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptDestroyKey([In] IntPtr phKey);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe bool CryptEncrypt([In] SafeKeyHandle phKey, [In] IntPtr hHash, [In] bool final, [In] uint dwFlags, [In] void* pbData, [In, Out] ref int dwDataLen, [In] int dwBufLen);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptGetKeyParam([In] SafeKeyHandle phKey, [In] uint dwParam, [In] IntPtr pbData, [In, Out] ref uint dwDataLen, [In] uint dwFlags);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe bool CryptImportKey([In] SafeProvHandle hProv, [In] void* pbData, [In] uint dwDataLen, [In] IntPtr hPubKey, [In] uint dwFlags, out SafeKeyHandle phKey);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptReleaseContext([In] IntPtr hProv, [In] uint dwFlags);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern unsafe bool CryptSetKeyParam([In] SafeKeyHandle phKey, [In] uint dwParam, [In] void* pbData, [In] uint dwFlags);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DuplicateTokenEx([In] SafeCloseHandle existingTokenHandle, [In] TokenAccessLevels desiredAccess, [In] IntPtr tokenAttributes, [In] SECURITY_IMPERSONATION_LEVEL impersonationLevel, [In] System.IdentityModel.TokenType tokenType, out SafeCloseHandle duplicateTokenHandle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr GetCurrentThread();
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetTokenInformation([In] IntPtr tokenHandle, [In] uint tokenInformationClass, [In] SafeHGlobalHandle tokenInformation, [In] uint tokenInformationLength, out uint returnLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool LogonUser([In] string lpszUserName, [In] string lpszDomain, [In] string lpszPassword, [In] uint dwLogonType, [In] uint dwLogonProvider, out SafeCloseHandle phToken);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern bool LookupPrivilegeValueW([In] string lpSystemName, [In] string lpName, out LUID Luid);
        [DllImport("secur32.dll", CharSet=CharSet.Auto)]
        internal static extern int LsaConnectUntrusted(out SafeLsaLogonProcessHandle lsaHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll", CharSet=CharSet.Auto)]
        internal static extern int LsaDeregisterLogonProcess([In] IntPtr handle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll")]
        internal static extern int LsaFreeReturnBuffer(IntPtr handle);
        [DllImport("secur32.dll", CharSet=CharSet.Auto)]
        internal static extern int LsaLogonUser([In] SafeLsaLogonProcessHandle LsaHandle, [In] ref UNICODE_INTPTR_STRING OriginName, [In] System.IdentityModel.SecurityLogonType LogonType, [In] uint AuthenticationPackage, [In] IntPtr AuthenticationInformation, [In] uint AuthenticationInformationLength, [In] IntPtr LocalGroups, [In] ref TOKEN_SOURCE SourceContext, out SafeLsaReturnBufferHandle ProfileBuffer, out uint ProfileBufferLength, out LUID LogonId, out SafeCloseHandle Token, out QUOTA_LIMITS Quotas, out int SubStatus);
        [DllImport("secur32.dll", CharSet=CharSet.Auto)]
        internal static extern int LsaLookupAuthenticationPackage([In] SafeLsaLogonProcessHandle lsaHandle, [In] ref UNICODE_INTPTR_STRING packageName, out uint authenticationPackage);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern int LsaNtStatusToWinError([In] int status);
        [DllImport("secur32.dll", CharSet=CharSet.Auto)]
        internal static extern int LsaRegisterLogonProcess([In] ref UNICODE_INTPTR_STRING logonProcessName, out SafeLsaLogonProcessHandle lsaHandle, out IntPtr securityMode);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool OpenProcessToken([In] IntPtr processToken, [In] TokenAccessLevels desiredAccess, out SafeCloseHandle tokenHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool OpenThreadToken([In] IntPtr threadHandle, [In] TokenAccessLevels desiredAccess, [In] bool openAsSelf, out SafeCloseHandle tokenHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool RevertToSelf();
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetThreadToken([In] IntPtr threadHandle, [In] SafeCloseHandle threadToken);
        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport("credui.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool SspiIsPromptingNeeded(uint ErrorOrNtStatus);
        [DllImport("credui.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern uint SspiPromptForCredentials(string pszTargetName, ref CREDUI_INFO pUiInfo, uint dwAuthError, string pszPackage, IntPtr authIdentity, out IntPtr ppAuthIdentity, [MarshalAs(UnmanagedType.Bool)] ref bool pfSave, uint dwFlags);
    }
}

