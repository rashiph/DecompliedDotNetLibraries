namespace System.Security.Cryptography
{
    using System;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public static class ProtectedMemory
    {
        [SecuritySafeCritical]
        public static void Protect(byte[] userData, MemoryProtectionScope scope)
        {
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
            VerifyScope(scope);
            if ((userData.Length == 0) || ((((long) userData.Length) % 0x10L) != 0L))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_DpApi_InvalidMemoryLength"));
            }
            uint dwFlags = (uint) scope;
            try
            {
                int status = System.Security.Cryptography.CAPI.SystemFunction040(userData, (uint) userData.Length, dwFlags);
                if (status < 0)
                {
                    throw new CryptographicException(System.Security.Cryptography.CAPI.CAPISafe.LsaNtStatusToWinError(status));
                }
            }
            catch (EntryPointNotFoundException)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
        }

        [SecuritySafeCritical]
        public static void Unprotect(byte[] encryptedData, MemoryProtectionScope scope)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException("encryptedData");
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
            VerifyScope(scope);
            if ((encryptedData.Length == 0) || ((((long) encryptedData.Length) % 0x10L) != 0L))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_DpApi_InvalidMemoryLength"));
            }
            uint dwFlags = (uint) scope;
            try
            {
                int status = System.Security.Cryptography.CAPI.SystemFunction041(encryptedData, (uint) encryptedData.Length, dwFlags);
                if (status < 0)
                {
                    throw new CryptographicException(System.Security.Cryptography.CAPI.CAPISafe.LsaNtStatusToWinError(status));
                }
            }
            catch (EntryPointNotFoundException)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
        }

        private static void VerifyScope(MemoryProtectionScope scope)
        {
            if (((scope != MemoryProtectionScope.SameProcess) && (scope != MemoryProtectionScope.CrossProcess)) && (scope != MemoryProtectionScope.SameLogon))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Arg_EnumIllegalVal"), new object[] { (int) scope }));
            }
        }
    }
}

