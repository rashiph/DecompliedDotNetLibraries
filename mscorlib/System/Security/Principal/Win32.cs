namespace System.Security.Principal
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class Win32
    {
        private static bool _LsaLookupNames2Supported;
        private static bool _WellKnownSidApisSupported;
        internal const int FALSE = 0;
        internal const int TRUE = 1;

        [SecuritySafeCritical]
        static Win32()
        {
            Win32Native.OSVERSIONINFO osVer = new Win32Native.OSVERSIONINFO();
            if (!Environment.GetVersion(osVer))
            {
                throw new SystemException(Environment.GetResourceString("InvalidOperation_GetVersion"));
            }
            if ((osVer.MajorVersion > 5) || (osVer.MinorVersion > 0))
            {
                _LsaLookupNames2Supported = true;
                _WellKnownSidApisSupported = true;
            }
            else
            {
                _LsaLookupNames2Supported = false;
                Win32Native.OSVERSIONINFOEX osversioninfoex = new Win32Native.OSVERSIONINFOEX();
                if (!Environment.GetVersionEx(osversioninfoex))
                {
                    throw new SystemException(Environment.GetResourceString("InvalidOperation_GetVersion"));
                }
                if (osversioninfoex.ServicePackMajor < 3)
                {
                    _WellKnownSidApisSupported = false;
                }
                else
                {
                    _WellKnownSidApisSupported = true;
                }
            }
        }

        [SecurityCritical]
        internal static byte[] ConvertIntPtrSidToByteArraySid(IntPtr binaryForm)
        {
            if (Marshal.ReadByte(binaryForm, 0) != SecurityIdentifier.Revision)
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_InvalidSidRevision"), "binaryForm");
            }
            byte num2 = Marshal.ReadByte(binaryForm, 1);
            if ((num2 < 0) || (num2 > SecurityIdentifier.MaxSubAuthorities))
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_InvalidNumberOfSubauthorities", new object[] { SecurityIdentifier.MaxSubAuthorities }), "binaryForm");
            }
            int length = 8 + (num2 * 4);
            byte[] destination = new byte[length];
            Marshal.Copy(binaryForm, destination, 0, length);
            return destination;
        }

        [SecurityCritical]
        internal static int CreateSidFromString(string stringSid, out byte[] resultSid)
        {
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (1 != Win32Native.ConvertStringSidToSid(stringSid, out zero))
                {
                    int num = Marshal.GetLastWin32Error();
                    resultSid = null;
                    return num;
                }
                resultSid = ConvertIntPtrSidToByteArraySid(zero);
            }
            finally
            {
                Win32Native.LocalFree(zero);
            }
            return 0;
        }

        [SecurityCritical]
        internal static int CreateWellKnownSid(WellKnownSidType sidType, SecurityIdentifier domainSid, out byte[] resultSid)
        {
            if (!WellKnownSidApisSupported)
            {
                throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
            }
            uint maxBinaryLength = (uint) SecurityIdentifier.MaxBinaryLength;
            resultSid = new byte[maxBinaryLength];
            if (Win32Native.CreateWellKnownSid((int) sidType, (domainSid == null) ? null : domainSid.BinaryForm, resultSid, ref maxBinaryLength) != 0)
            {
                return 0;
            }
            resultSid = null;
            return Marshal.GetLastWin32Error();
        }

        [SecurityCritical]
        internal static int GetWindowsAccountDomainSid(SecurityIdentifier sid, out SecurityIdentifier resultSid)
        {
            if (!WellKnownSidApisSupported)
            {
                throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
            }
            byte[] binaryForm = new byte[sid.BinaryLength];
            sid.GetBinaryForm(binaryForm, 0);
            uint maxBinaryLength = (uint) SecurityIdentifier.MaxBinaryLength;
            byte[] buffer2 = new byte[maxBinaryLength];
            if (Win32Native.GetWindowsAccountDomainSid(binaryForm, buffer2, ref maxBinaryLength) != 0)
            {
                resultSid = new SecurityIdentifier(buffer2, 0);
                return 0;
            }
            resultSid = null;
            return Marshal.GetLastWin32Error();
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int ImpersonateLoggedOnUser(SafeTokenHandle hToken);
        [SecurityCritical]
        internal static unsafe void InitializeReferencedDomainsPointer(SafeLsaMemoryHandle referencedDomains)
        {
            referencedDomains.Initialize((ulong) Marshal.SizeOf(typeof(Win32Native.LSA_REFERENCED_DOMAIN_LIST)));
            Win32Native.LSA_REFERENCED_DOMAIN_LIST lsa_referenced_domain_list = referencedDomains.Read<Win32Native.LSA_REFERENCED_DOMAIN_LIST>(0L);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                referencedDomains.AcquirePointer(ref pointer);
                if (!lsa_referenced_domain_list.Domains.IsNull())
                {
                    Win32Native.LSA_TRUST_INFORMATION* domains = (Win32Native.LSA_TRUST_INFORMATION*) lsa_referenced_domain_list.Domains;
                    domains += lsa_referenced_domain_list.Entries;
                    long num = (long) ((domains - pointer) / 1);
                    referencedDomains.Initialize((ulong) num);
                }
            }
            finally
            {
                if (pointer != null)
                {
                    referencedDomains.ReleasePointer();
                }
            }
        }

        [SecurityCritical]
        internal static bool IsEqualDomainSid(SecurityIdentifier sid1, SecurityIdentifier sid2)
        {
            bool flag;
            if (!WellKnownSidApisSupported)
            {
                throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
            }
            if ((sid1 == null) || (sid2 == null))
            {
                return false;
            }
            byte[] binaryForm = new byte[sid1.BinaryLength];
            sid1.GetBinaryForm(binaryForm, 0);
            byte[] buffer2 = new byte[sid2.BinaryLength];
            sid2.GetBinaryForm(buffer2, 0);
            return ((Win32Native.IsEqualDomainSid(binaryForm, buffer2, out flag) != 0) && flag);
        }

        [SecurityCritical]
        internal static bool IsWellKnownSid(SecurityIdentifier sid, WellKnownSidType type)
        {
            if (!WellKnownSidApisSupported)
            {
                throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
            }
            byte[] binaryForm = new byte[sid.BinaryLength];
            sid.GetBinaryForm(binaryForm, 0);
            if (Win32Native.IsWellKnownSid(binaryForm, (int) type) == 0)
            {
                return false;
            }
            return true;
        }

        [SecurityCritical]
        internal static SafeLsaPolicyHandle LsaOpenPolicy(string systemName, PolicyRights rights)
        {
            SafeLsaPolicyHandle handle;
            Win32Native.LSA_OBJECT_ATTRIBUTES lsa_object_attributes;
            lsa_object_attributes.Length = Marshal.SizeOf(typeof(Win32Native.LSA_OBJECT_ATTRIBUTES));
            lsa_object_attributes.RootDirectory = IntPtr.Zero;
            lsa_object_attributes.ObjectName = IntPtr.Zero;
            lsa_object_attributes.Attributes = 0;
            lsa_object_attributes.SecurityDescriptor = IntPtr.Zero;
            lsa_object_attributes.SecurityQualityOfService = IntPtr.Zero;
            uint num = Win32Native.LsaOpenPolicy(systemName, ref lsa_object_attributes, (int) rights, out handle);
            if (num == 0)
            {
                return handle;
            }
            if (num == 0xc0000022)
            {
                throw new UnauthorizedAccessException();
            }
            if ((num != 0xc000009a) && (num != 0xc0000017))
            {
                throw new SystemException(Win32Native.GetMessage(Win32Native.LsaNtStatusToWinError((int) num)));
            }
            throw new OutOfMemoryException();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int OpenThreadToken(TokenAccessLevels dwDesiredAccess, WinSecurityContext OpenAs, out SafeTokenHandle phThreadToken);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int RevertToSelf();
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int SetThreadToken(SafeTokenHandle hToken);

        internal static bool LsaLookupNames2Supported
        {
            get
            {
                return _LsaLookupNames2Supported;
            }
        }

        internal static bool WellKnownSidApisSupported
        {
            get
            {
                return _WellKnownSidApisSupported;
            }
        }
    }
}

