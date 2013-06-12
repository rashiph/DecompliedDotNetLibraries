namespace System.Security.Cryptography.X509Certificates
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;

    internal static class X509Utils
    {
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _AddCertificateToStore(SafeCertStoreHandle safeCertStoreHandle, SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _DuplicateCertContext(IntPtr handle, ref SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern byte[] _ExportCertificatesToBlob(SafeCertStoreHandle safeCertStoreHandle, X509ContentType contentType, IntPtr password);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int _GetAlgIdFromOid(string oid, OidGroup group);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern byte[] _GetCertRawData(SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _GetDateNotAfter(SafeCertContextHandle safeCertContext, ref Win32Native.FILE_TIME fileTime);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _GetDateNotBefore(SafeCertContextHandle safeCertContext, ref Win32Native.FILE_TIME fileTime);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string _GetFriendlyNameFromOid(string oid);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string _GetIssuerName(SafeCertContextHandle safeCertContext, bool legacyV1Mode);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string _GetOidFromFriendlyName(string oid, OidGroup group);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string _GetPublicKeyOid(SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern byte[] _GetPublicKeyParameters(SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern byte[] _GetPublicKeyValue(SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern byte[] _GetSerialNumber(SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string _GetSubjectInfo(SafeCertContextHandle safeCertContext, uint displayType, bool legacyV1Mode);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern byte[] _GetThumbprint(SafeCertContextHandle safeCertContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _LoadCertFromBlob(byte[] rawData, IntPtr password, uint dwFlags, bool persistKeySet, ref SafeCertContextHandle pCertCtx);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _LoadCertFromFile(string fileName, IntPtr password, uint dwFlags, bool persistKeySet, ref SafeCertContextHandle pCertCtx);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _OpenX509Store(uint storeType, uint flags, string storeName, ref SafeCertStoreHandle safeCertStoreHandle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern uint _QueryCertBlobType(byte[] rawData);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern uint _QueryCertFileType(string fileName);
        [SecurityCritical]
        internal static SafeCertStoreHandle ExportCertToMemoryStore(X509Certificate certificate)
        {
            SafeCertStoreHandle invalidHandle = SafeCertStoreHandle.InvalidHandle;
            _OpenX509Store(2, 0x2200, null, ref invalidHandle);
            _AddCertificateToStore(invalidHandle, certificate.CertContext);
            return invalidHandle;
        }

        internal static X509ContentType MapContentType(uint contentType)
        {
            switch (contentType)
            {
                case 1:
                    return X509ContentType.Cert;

                case 4:
                    return X509ContentType.SerializedStore;

                case 5:
                    return X509ContentType.SerializedCert;

                case 8:
                case 9:
                    return X509ContentType.Pkcs7;

                case 10:
                    return X509ContentType.Authenticode;

                case 12:
                    return X509ContentType.Pfx;
            }
            return X509ContentType.Unknown;
        }

        internal static uint MapKeyStorageFlags(X509KeyStorageFlags keyStorageFlags)
        {
            if ((keyStorageFlags & ~(X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserProtected | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.UserKeySet)) != X509KeyStorageFlags.DefaultKeySet)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "keyStorageFlags");
            }
            uint num = 0;
            if ((keyStorageFlags & X509KeyStorageFlags.UserKeySet) == X509KeyStorageFlags.UserKeySet)
            {
                num |= 0x1000;
            }
            else if ((keyStorageFlags & X509KeyStorageFlags.MachineKeySet) == X509KeyStorageFlags.MachineKeySet)
            {
                num |= 0x20;
            }
            if ((keyStorageFlags & X509KeyStorageFlags.Exportable) == X509KeyStorageFlags.Exportable)
            {
                num |= 1;
            }
            if ((keyStorageFlags & X509KeyStorageFlags.UserProtected) == X509KeyStorageFlags.UserProtected)
            {
                num |= 2;
            }
            return num;
        }

        internal static int OidToAlgId(string oid)
        {
            return OidToAlgId(oid, OidGroup.AllGroups);
        }

        internal static int OidToAlgId(string oid, OidGroup group)
        {
            if (oid == null)
            {
                return 0x8004;
            }
            string str = CryptoConfig.MapNameToOID(oid);
            if (str == null)
            {
                str = oid;
            }
            return OidToAlgIdStrict(str, group);
        }

        [SecuritySafeCritical]
        internal static int OidToAlgIdStrict(string oid, OidGroup group)
        {
            int num = 0;
            if (string.Equals(oid, "2.16.840.1.101.3.4.2.1", StringComparison.Ordinal))
            {
                num = 0x800c;
            }
            else if (string.Equals(oid, "2.16.840.1.101.3.4.2.2", StringComparison.Ordinal))
            {
                num = 0x800d;
            }
            else if (string.Equals(oid, "2.16.840.1.101.3.4.2.3", StringComparison.Ordinal))
            {
                num = 0x800e;
            }
            else
            {
                num = _GetAlgIdFromOid(oid, group);
            }
            if ((num == 0) || (num == -1))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidOID"));
            }
            return num;
        }

        [SecurityCritical]
        internal static IntPtr PasswordToHGlobalUni(object password)
        {
            if (password != null)
            {
                string s = password as string;
                if (s != null)
                {
                    return Marshal.StringToHGlobalUni(s);
                }
                SecureString str2 = password as SecureString;
                if (str2 != null)
                {
                    return Marshal.SecureStringToGlobalAllocUnicode(str2);
                }
            }
            return IntPtr.Zero;
        }
    }
}

