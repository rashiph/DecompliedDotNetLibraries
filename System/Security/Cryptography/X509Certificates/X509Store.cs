namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    public sealed class X509Store
    {
        private StoreLocation m_location;
        private System.Security.Cryptography.SafeCertStoreHandle m_safeCertStoreHandle;
        private string m_storeName;

        public X509Store() : this("MY", StoreLocation.CurrentUser)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public X509Store(IntPtr storeHandle)
        {
            this.m_safeCertStoreHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            if (storeHandle == IntPtr.Zero)
            {
                throw new ArgumentNullException("storeHandle");
            }
            this.m_safeCertStoreHandle = CAPISafe.CertDuplicateStore(storeHandle);
            if ((this.m_safeCertStoreHandle == null) || this.m_safeCertStoreHandle.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidStoreHandle"), "storeHandle");
            }
        }

        public X509Store(StoreLocation storeLocation) : this("MY", storeLocation)
        {
        }

        public X509Store(StoreName storeName) : this(storeName, StoreLocation.CurrentUser)
        {
        }

        public X509Store(string storeName) : this(storeName, StoreLocation.CurrentUser)
        {
        }

        public X509Store(StoreName storeName, StoreLocation storeLocation)
        {
            this.m_safeCertStoreHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            if ((storeLocation != StoreLocation.CurrentUser) && (storeLocation != StoreLocation.LocalMachine))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "storeLocation" }));
            }
            switch (storeName)
            {
                case StoreName.AddressBook:
                    this.m_storeName = "AddressBook";
                    break;

                case StoreName.AuthRoot:
                    this.m_storeName = "AuthRoot";
                    break;

                case StoreName.CertificateAuthority:
                    this.m_storeName = "CA";
                    break;

                case StoreName.Disallowed:
                    this.m_storeName = "Disallowed";
                    break;

                case StoreName.My:
                    this.m_storeName = "My";
                    break;

                case StoreName.Root:
                    this.m_storeName = "Root";
                    break;

                case StoreName.TrustedPeople:
                    this.m_storeName = "TrustedPeople";
                    break;

                case StoreName.TrustedPublisher:
                    this.m_storeName = "TrustedPublisher";
                    break;

                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "storeName" }));
            }
            this.m_location = storeLocation;
        }

        public X509Store(string storeName, StoreLocation storeLocation)
        {
            this.m_safeCertStoreHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            if ((storeLocation != StoreLocation.CurrentUser) && (storeLocation != StoreLocation.LocalMachine))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "storeLocation" }));
            }
            this.m_storeName = storeName;
            this.m_location = storeLocation;
        }

        public void Add(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            if (((this.m_safeCertStoreHandle == null) || this.m_safeCertStoreHandle.IsInvalid) || this.m_safeCertStoreHandle.IsClosed)
            {
                throw new CryptographicException(SR.GetString("Cryptography_X509_StoreNotOpen"));
            }
            if (!CAPI.CertAddCertificateContextToStore(this.m_safeCertStoreHandle, certificate.CertContext, 5, System.Security.Cryptography.SafeCertContextHandle.InvalidHandle))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

        public void AddRange(X509Certificate2Collection certificates)
        {
            if (certificates == null)
            {
                throw new ArgumentNullException("certificates");
            }
            int num = 0;
            try
            {
                X509Certificate2Enumerator enumerator = certificates.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Certificate2 current = enumerator.Current;
                    this.Add(current);
                    num++;
                }
            }
            catch
            {
                for (int i = 0; i < num; i++)
                {
                    this.Remove(certificates[i]);
                }
                throw;
            }
        }

        public void Close()
        {
            if ((this.m_safeCertStoreHandle != null) && !this.m_safeCertStoreHandle.IsClosed)
            {
                this.m_safeCertStoreHandle.Dispose();
            }
        }

        public void Open(OpenFlags flags)
        {
            if ((this.m_location != StoreLocation.CurrentUser) && (this.m_location != StoreLocation.LocalMachine))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "m_location" }));
            }
            uint dwFlags = System.Security.Cryptography.X509Certificates.X509Utils.MapX509StoreFlags(this.m_location, flags);
            if (!this.m_safeCertStoreHandle.IsInvalid)
            {
                this.m_safeCertStoreHandle.Dispose();
            }
            this.m_safeCertStoreHandle = CAPI.CertOpenStore(new IntPtr(10L), 0x10001, IntPtr.Zero, dwFlags, this.m_storeName);
            if ((this.m_safeCertStoreHandle == null) || this.m_safeCertStoreHandle.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            CAPISafe.CertControlStore(this.m_safeCertStoreHandle, 0, 4, IntPtr.Zero);
        }

        public void Remove(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            RemoveCertificateFromStore(this.m_safeCertStoreHandle, certificate.CertContext);
        }

        private static void RemoveCertificateFromStore(System.Security.Cryptography.SafeCertStoreHandle safeCertStoreHandle, System.Security.Cryptography.SafeCertContextHandle safeCertContext)
        {
            if ((safeCertContext != null) && !safeCertContext.IsInvalid)
            {
                if (((safeCertStoreHandle == null) || safeCertStoreHandle.IsInvalid) || safeCertStoreHandle.IsClosed)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_X509_StoreNotOpen"));
                }
                System.Security.Cryptography.SafeCertContextHandle handle = CAPI.CertFindCertificateInStore(safeCertStoreHandle, 0x10001, 0, 0xd0000, safeCertContext.DangerousGetHandle(), System.Security.Cryptography.SafeCertContextHandle.InvalidHandle);
                if ((handle != null) && !handle.IsInvalid)
                {
                    GC.SuppressFinalize(handle);
                    if (!CAPI.CertDeleteCertificateFromStore(handle))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                }
            }
        }

        public void RemoveRange(X509Certificate2Collection certificates)
        {
            if (certificates == null)
            {
                throw new ArgumentNullException("certificates");
            }
            int num = 0;
            try
            {
                X509Certificate2Enumerator enumerator = certificates.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Certificate2 current = enumerator.Current;
                    this.Remove(current);
                    num++;
                }
            }
            catch
            {
                for (int i = 0; i < num; i++)
                {
                    this.Add(certificates[i]);
                }
                throw;
            }
        }

        public X509Certificate2Collection Certificates
        {
            get
            {
                if (!this.m_safeCertStoreHandle.IsInvalid && !this.m_safeCertStoreHandle.IsClosed)
                {
                    return System.Security.Cryptography.X509Certificates.X509Utils.GetCertificates(this.m_safeCertStoreHandle);
                }
                return new X509Certificate2Collection();
            }
        }

        public StoreLocation Location
        {
            get
            {
                return this.m_location;
            }
        }

        public string Name
        {
            get
            {
                return this.m_storeName;
            }
        }

        public IntPtr StoreHandle
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (((this.m_safeCertStoreHandle == null) || this.m_safeCertStoreHandle.IsInvalid) || this.m_safeCertStoreHandle.IsClosed)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_X509_StoreNotOpen"));
                }
                return this.m_safeCertStoreHandle.DangerousGetHandle();
            }
        }
    }
}

