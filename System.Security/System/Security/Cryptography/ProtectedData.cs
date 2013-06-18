namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public static class ProtectedData
    {
        [SecuritySafeCritical]
        public static unsafe byte[] Protect(byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
        {
            byte[] buffer2;
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
            GCHandle handle = new GCHandle();
            GCHandle handle2 = new GCHandle();
            System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                handle = GCHandle.Alloc(userData, GCHandleType.Pinned);
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob2 = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB {
                    cbData = (uint) userData.Length,
                    pbData = handle.AddrOfPinnedObject()
                };
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob3 = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB();
                if (optionalEntropy != null)
                {
                    handle2 = GCHandle.Alloc(optionalEntropy, GCHandleType.Pinned);
                    cryptoapi_blob3.cbData = (uint) optionalEntropy.Length;
                    cryptoapi_blob3.pbData = handle2.AddrOfPinnedObject();
                }
                uint dwFlags = 1;
                if (scope == DataProtectionScope.LocalMachine)
                {
                    dwFlags |= 4;
                }
                if (!System.Security.Cryptography.CAPI.CryptProtectData(new IntPtr((void*) &cryptoapi_blob2), string.Empty, new IntPtr((void*) &cryptoapi_blob3), IntPtr.Zero, IntPtr.Zero, dwFlags, new IntPtr((void*) &cryptoapi_blob)))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if (System.Security.Cryptography.CAPI.ErrorMayBeCausedByUnloadedProfile(errorCode))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_DpApi_ProfileMayNotBeLoaded"));
                    }
                    throw new CryptographicException(errorCode);
                }
                if (cryptoapi_blob.pbData == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                byte[] destination = new byte[cryptoapi_blob.cbData];
                Marshal.Copy(cryptoapi_blob.pbData, destination, 0, destination.Length);
                buffer2 = destination;
            }
            catch (EntryPointNotFoundException)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
                if (handle2.IsAllocated)
                {
                    handle2.Free();
                }
                if (cryptoapi_blob.pbData != IntPtr.Zero)
                {
                    System.Security.Cryptography.CAPI.CAPISafe.ZeroMemory(cryptoapi_blob.pbData, cryptoapi_blob.cbData);
                    System.Security.Cryptography.CAPI.CAPISafe.LocalFree(cryptoapi_blob.pbData);
                }
            }
            return buffer2;
        }

        [SecuritySafeCritical]
        public static unsafe byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
        {
            byte[] buffer2;
            if (encryptedData == null)
            {
                throw new ArgumentNullException("encryptedData");
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
            GCHandle handle = new GCHandle();
            GCHandle handle2 = new GCHandle();
            System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                handle = GCHandle.Alloc(encryptedData, GCHandleType.Pinned);
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob2 = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB {
                    cbData = (uint) encryptedData.Length,
                    pbData = handle.AddrOfPinnedObject()
                };
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob3 = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB();
                if (optionalEntropy != null)
                {
                    handle2 = GCHandle.Alloc(optionalEntropy, GCHandleType.Pinned);
                    cryptoapi_blob3.cbData = (uint) optionalEntropy.Length;
                    cryptoapi_blob3.pbData = handle2.AddrOfPinnedObject();
                }
                uint dwFlags = 1;
                if (scope == DataProtectionScope.LocalMachine)
                {
                    dwFlags |= 4;
                }
                if (!System.Security.Cryptography.CAPI.CryptUnprotectData(new IntPtr((void*) &cryptoapi_blob2), IntPtr.Zero, new IntPtr((void*) &cryptoapi_blob3), IntPtr.Zero, IntPtr.Zero, dwFlags, new IntPtr((void*) &cryptoapi_blob)))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                if (cryptoapi_blob.pbData == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                byte[] destination = new byte[cryptoapi_blob.cbData];
                Marshal.Copy(cryptoapi_blob.pbData, destination, 0, destination.Length);
                buffer2 = destination;
            }
            catch (EntryPointNotFoundException)
            {
                throw new NotSupportedException(SecurityResources.GetResourceString("NotSupported_PlatformRequiresNT"));
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
                if (handle2.IsAllocated)
                {
                    handle2.Free();
                }
                if (cryptoapi_blob.pbData != IntPtr.Zero)
                {
                    System.Security.Cryptography.CAPI.CAPISafe.ZeroMemory(cryptoapi_blob.pbData, cryptoapi_blob.cbData);
                    System.Security.Cryptography.CAPI.CAPISafe.LocalFree(cryptoapi_blob.pbData);
                }
            }
            return buffer2;
        }
    }
}

