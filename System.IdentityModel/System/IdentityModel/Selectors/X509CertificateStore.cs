namespace System.IdentityModel.Selectors
{
    using System;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    internal class X509CertificateStore
    {
        private System.IdentityModel.SafeCertStoreHandle certStoreHandle = System.IdentityModel.SafeCertStoreHandle.InvalidHandle;
        private StoreLocation storeLocation;
        private string storeName;

        public X509CertificateStore(StoreName storeName, StoreLocation storeLocation)
        {
            switch (storeName)
            {
                case StoreName.AddressBook:
                    this.storeName = "AddressBook";
                    break;

                case StoreName.AuthRoot:
                    this.storeName = "AuthRoot";
                    break;

                case StoreName.CertificateAuthority:
                    this.storeName = "CA";
                    break;

                case StoreName.Disallowed:
                    this.storeName = "Disallowed";
                    break;

                case StoreName.My:
                    this.storeName = "My";
                    break;

                case StoreName.Root:
                    this.storeName = "Root";
                    break;

                case StoreName.TrustedPeople:
                    this.storeName = "TrustedPeople";
                    break;

                case StoreName.TrustedPublisher:
                    this.storeName = "TrustedPublisher";
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("storeName", (int) storeName, typeof(StoreName)));
            }
            if ((storeLocation != StoreLocation.CurrentUser) && (storeLocation != StoreLocation.LocalMachine))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("storeLocation", System.IdentityModel.SR.GetString("X509CertStoreLocationNotValid")));
            }
            this.storeLocation = storeLocation;
        }

        private bool BinaryMatches(byte[] src, byte[] dst)
        {
            if (src.Length != dst.Length)
            {
                return false;
            }
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] != dst[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void Close()
        {
            this.certStoreHandle.Dispose();
        }

        public X509Certificate2Collection Find(X509FindType findType, object findValue, bool validOnly)
        {
            SafeHGlobalHandle invalidHandle = SafeHGlobalHandle.InvalidHandle;
            System.IdentityModel.SafeCertContextHandle pPrevCertContext = System.IdentityModel.SafeCertContextHandle.InvalidHandle;
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            SafeHGlobalHandle handle3 = SafeHGlobalHandle.InvalidHandle;
            try
            {
                uint num;
                string str;
                byte[] buffer;
                System.IdentityModel.CAPI.CRYPTOAPI_BLOB cryptoapi_blob;
                switch (findType)
                {
                    case X509FindType.FindByThumbprint:
                        buffer = findValue as byte[];
                        if (buffer == null)
                        {
                            str = findValue as string;
                            if (str == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("X509FindValueMismatchMulti", new object[] { findType, typeof(string), typeof(byte[]), findValue.GetType() })));
                            }
                            goto Label_011A;
                        }
                        goto Label_0123;

                    case X509FindType.FindBySubjectName:
                        str = findValue as string;
                        if (str == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("X509FindValueMismatch", new object[] { findType, typeof(string), findValue.GetType() })));
                        }
                        break;

                    case X509FindType.FindBySubjectDistinguishedName:
                        if (!(findValue is string))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("X509FindValueMismatch", new object[] { findType, typeof(string), findValue.GetType() })));
                        }
                        goto Label_01C4;

                    case X509FindType.FindByIssuerName:
                        str = findValue as string;
                        if (str == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("X509FindValueMismatch", new object[] { findType, typeof(string), findValue.GetType() })));
                        }
                        goto Label_021D;

                    case X509FindType.FindByIssuerDistinguishedName:
                        if (!(findValue is string))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("X509FindValueMismatch", new object[] { findType, typeof(string), findValue.GetType() })));
                        }
                        goto Label_027E;

                    case X509FindType.FindBySerialNumber:
                        buffer = findValue as byte[];
                        if (buffer == null)
                        {
                            str = findValue as string;
                            if (str == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("X509FindValueMismatchMulti", new object[] { findType, typeof(string), typeof(byte[]), findValue.GetType() })));
                            }
                            goto Label_02F4;
                        }
                        goto Label_033C;

                    case X509FindType.FindBySubjectKeyIdentifier:
                        buffer = findValue as byte[];
                        if (buffer == null)
                        {
                            str = findValue as string;
                            if (str == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("X509FindValueMismatchMulti", new object[] { findType, typeof(string), typeof(byte[]), findValue.GetType() })));
                            }
                            buffer = System.IdentityModel.SecurityUtils.DecodeHexString(str);
                        }
                        findValue = buffer;
                        num = 0;
                        goto Label_03F4;

                    default:
                    {
                        X509Store store = new X509Store(this.certStoreHandle.DangerousGetHandle());
                        try
                        {
                            return store.Certificates.Find(findType, findValue, validOnly);
                        }
                        finally
                        {
                            store.Close();
                        }
                        goto Label_03F4;
                    }
                }
                num = 0x80007;
                invalidHandle = SafeHGlobalHandle.AllocHGlobal(str);
                goto Label_03F4;
            Label_011A:
                buffer = System.IdentityModel.SecurityUtils.DecodeHexString(str);
            Label_0123:
                cryptoapi_blob = new System.IdentityModel.CAPI.CRYPTOAPI_BLOB();
                handle3 = SafeHGlobalHandle.AllocHGlobal(buffer);
                cryptoapi_blob.pbData = handle3.DangerousGetHandle();
                cryptoapi_blob.cbData = (uint) buffer.Length;
                num = 0x10000;
                Marshal.StructureToPtr(cryptoapi_blob, SafeHGlobalHandle.AllocHGlobal(System.IdentityModel.CAPI.CRYPTOAPI_BLOB.Size).DangerousGetHandle(), false);
                goto Label_03F4;
            Label_01C4:
                num = 0;
                goto Label_03F4;
            Label_021D:
                num = 0x80004;
                invalidHandle = SafeHGlobalHandle.AllocHGlobal(str);
                goto Label_03F4;
            Label_027E:
                num = 0;
                goto Label_03F4;
            Label_02F4:
                buffer = System.IdentityModel.SecurityUtils.DecodeHexString(str);
                int length = buffer.Length;
                int index = 0;
                for (int i = length - 1; index < (buffer.Length / 2); i--)
                {
                    byte num5 = buffer[index];
                    buffer[index] = buffer[i];
                    buffer[i] = num5;
                    index++;
                }
            Label_033C:
                findValue = buffer;
                num = 0;
            Label_03F4:
                pPrevCertContext = System.IdentityModel.CAPI.CertFindCertificateInStore(this.certStoreHandle, 0x10001, 0, num, invalidHandle, pPrevCertContext);
                while ((pPrevCertContext != null) && !pPrevCertContext.IsInvalid)
                {
                    X509Certificate2 certificate;
                    if (this.TryGetMatchingX509Certificate(pPrevCertContext.DangerousGetHandle(), findType, num, findValue, validOnly, out certificate))
                    {
                        certificates.Add(certificate);
                    }
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        continue;
                    }
                    finally
                    {
                        GC.SuppressFinalize(pPrevCertContext);
                        pPrevCertContext = System.IdentityModel.CAPI.CertFindCertificateInStore(this.certStoreHandle, 0x10001, 0, num, invalidHandle, pPrevCertContext);
                    }
                }
            }
            finally
            {
                if (pPrevCertContext != null)
                {
                    pPrevCertContext.Close();
                }
                invalidHandle.Close();
                handle3.Close();
            }
            return certificates;
        }

        private uint MapX509StoreFlags(StoreLocation storeLocation, OpenFlags flags)
        {
            uint num = 0;
            switch (((uint) (flags & (OpenFlags.MaxAllowed | OpenFlags.ReadWrite))))
            {
                case 0:
                    num |= 0x8000;
                    break;

                case 2:
                    num |= 0x1000;
                    break;
            }
            if ((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly)
            {
                num |= 0x4000;
            }
            if ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived)
            {
                num |= 0x200;
            }
            if (storeLocation == StoreLocation.LocalMachine)
            {
                return (num | 0x20000);
            }
            if (storeLocation == StoreLocation.CurrentUser)
            {
                num |= 0x10000;
            }
            return num;
        }

        public void Open(OpenFlags openFlags)
        {
            uint dwFlags = this.MapX509StoreFlags(this.storeLocation, openFlags);
            System.IdentityModel.SafeCertStoreHandle handle = System.IdentityModel.CAPI.CertOpenStore(new IntPtr(10L), 0x10001, IntPtr.Zero, dwFlags, this.storeName);
            if ((handle == null) || handle.IsInvalid)
            {
                int hr = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(hr));
            }
            this.certStoreHandle = handle;
        }

        private bool TryGetMatchingX509Certificate(IntPtr certContext, X509FindType findType, uint dwFindType, object findValue, bool validOnly, out X509Certificate2 cert)
        {
            cert = new X509Certificate2(certContext);
            if (dwFindType == 0)
            {
                switch (findType)
                {
                    case X509FindType.FindBySubjectDistinguishedName:
                        if (string.Compare((string) findValue, cert.SubjectName.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            break;
                        }
                        cert.Reset();
                        cert = null;
                        return false;

                    case X509FindType.FindByIssuerDistinguishedName:
                        if (string.Compare((string) findValue, cert.IssuerName.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            break;
                        }
                        cert.Reset();
                        cert = null;
                        return false;

                    case X509FindType.FindBySerialNumber:
                        if (this.BinaryMatches((byte[]) findValue, cert.GetSerialNumber()))
                        {
                            break;
                        }
                        cert.Reset();
                        cert = null;
                        return false;

                    case X509FindType.FindBySubjectKeyIdentifier:
                    {
                        X509SubjectKeyIdentifierExtension extension = cert.Extensions["2.5.29.14"] as X509SubjectKeyIdentifierExtension;
                        if ((extension == null) || !this.BinaryMatches((byte[]) findValue, extension.RawData))
                        {
                            cert.Reset();
                            cert = null;
                            return false;
                        }
                        break;
                    }
                }
            }
            if (validOnly && !new X509Chain(false) { ChainPolicy = { RevocationMode = X509RevocationMode.NoCheck, RevocationFlag = X509RevocationFlag.ExcludeRoot } }.Build(cert))
            {
                cert.Reset();
                cert = null;
                return false;
            }
            return (cert != null);
        }
    }
}

