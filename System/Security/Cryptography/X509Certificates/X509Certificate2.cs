namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public class X509Certificate2 : X509Certificate
    {
        private X509ExtensionCollection m_extensions;
        private X500DistinguishedName m_issuerName;
        private DateTime m_notAfter;
        private DateTime m_notBefore;
        private AsymmetricAlgorithm m_privateKey;
        private System.Security.Cryptography.X509Certificates.PublicKey m_publicKey;
        private System.Security.Cryptography.SafeCertContextHandle m_safeCertContext;
        private Oid m_signatureAlgorithm;
        private X500DistinguishedName m_subjectName;
        private int m_version;
        private static int s_publicKeyOffset;

        public X509Certificate2()
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
        }

        public X509Certificate2(byte[] rawData) : base(rawData)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public X509Certificate2(IntPtr handle) : base(handle)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(X509Certificate certificate) : base(certificate)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(string fileName) : base(fileName)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected X509Certificate2(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(byte[] rawData, SecureString password) : base(rawData, password)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(string fileName, SecureString password) : base(fileName, password)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(byte[] rawData, string password) : base(rawData, password)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(string fileName, string password) : base(fileName, password)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags) : base(rawData, password, keyStorageFlags)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) : base(fileName, password, keyStorageFlags)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags) : base(rawData, password, keyStorageFlags)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        public X509Certificate2(string fileName, string password, X509KeyStorageFlags keyStorageFlags) : base(fileName, password, keyStorageFlags)
        {
            this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        private void AppendPrivateKeyInfo(StringBuilder sb)
        {
            CspKeyContainerInfo info = null;
            try
            {
                if (this.HasPrivateKey)
                {
                    CspParameters parameters = new CspParameters();
                    if (GetPrivateKeyInfo(this.m_safeCertContext, ref parameters))
                    {
                        info = new CspKeyContainerInfo(parameters);
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (CryptographicException)
            {
            }
            if (info != null)
            {
                sb.Append(Environment.NewLine + Environment.NewLine + "[Private Key]");
                sb.Append(Environment.NewLine + "  Key Store: ");
                sb.Append(info.MachineKeyStore ? "Machine" : "User");
                sb.Append(Environment.NewLine + "  Provider Name: ");
                sb.Append(info.ProviderName);
                sb.Append(Environment.NewLine + "  Provider type: ");
                sb.Append(info.ProviderType);
                sb.Append(Environment.NewLine + "  Key Spec: ");
                sb.Append(info.KeyNumber);
                sb.Append(Environment.NewLine + "  Key Container Name: ");
                sb.Append(info.KeyContainerName);
                try
                {
                    string uniqueKeyContainerName = info.UniqueKeyContainerName;
                    sb.Append(Environment.NewLine + "  Unique Key Container Name: ");
                    sb.Append(uniqueKeyContainerName);
                }
                catch (CryptographicException)
                {
                }
                catch (NotSupportedException)
                {
                }
                bool hardwareDevice = false;
                try
                {
                    hardwareDevice = info.HardwareDevice;
                    sb.Append(Environment.NewLine + "  Hardware Device: ");
                    sb.Append(hardwareDevice);
                }
                catch (CryptographicException)
                {
                }
                try
                {
                    hardwareDevice = info.Removable;
                    sb.Append(Environment.NewLine + "  Removable: ");
                    sb.Append(hardwareDevice);
                }
                catch (CryptographicException)
                {
                }
                try
                {
                    hardwareDevice = info.Protected;
                    sb.Append(Environment.NewLine + "  Protected: ");
                    sb.Append(hardwareDevice);
                }
                catch (CryptographicException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        public static X509ContentType GetCertContentType(byte[] rawData)
        {
            if ((rawData == null) || (rawData.Length == 0))
            {
                throw new ArgumentException(SR.GetString("Arg_EmptyOrNullArray"), "rawData");
            }
            return System.Security.Cryptography.X509Certificates.X509Utils.MapContentType(QueryCertBlobType(rawData));
        }

        public static X509ContentType GetCertContentType(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            string fullPath = Path.GetFullPath(fileName);
            new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
            return System.Security.Cryptography.X509Certificates.X509Utils.MapContentType(QueryCertFileType(fileName));
        }

        public unsafe string GetNameInfo(X509NameType nameType, bool forIssuer)
        {
            uint dwFlags = forIssuer ? 1 : 0;
            uint dwDisplayType = System.Security.Cryptography.X509Certificates.X509Utils.MapNameType(nameType);
            switch (dwDisplayType)
            {
                case 1:
                    return CAPI.GetCertNameInfo(this.m_safeCertContext, dwFlags, dwDisplayType);

                case 4:
                    return CAPI.GetCertNameInfo(this.m_safeCertContext, dwFlags, dwDisplayType);
            }
            string str = string.Empty;
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) this.m_safeCertContext.DangerousGetHandle());
            CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
            IntPtr[] ptrArray = new IntPtr[] { CAPISafe.CertFindExtension(forIssuer ? "2.5.29.8" : "2.5.29.7", cert_info.cExtension, cert_info.rgExtension), CAPISafe.CertFindExtension(forIssuer ? "2.5.29.18" : "2.5.29.17", cert_info.cExtension, cert_info.rgExtension) };
            for (int i = 0; i < ptrArray.Length; i++)
            {
                if (ptrArray[i] != IntPtr.Zero)
                {
                    CAPIBase.CERT_EXTENSION cert_extension = (CAPIBase.CERT_EXTENSION) Marshal.PtrToStructure(ptrArray[i], typeof(CAPIBase.CERT_EXTENSION));
                    byte[] destination = new byte[cert_extension.Value.cbData];
                    Marshal.Copy(cert_extension.Value.pbData, destination, 0, destination.Length);
                    uint cbDecodedValue = 0;
                    SafeLocalAllocHandle decodedValue = null;
                    SafeLocalAllocHandle handle2 = System.Security.Cryptography.X509Certificates.X509Utils.StringToAnsiPtr(cert_extension.pszObjId);
                    bool flag = CAPI.DecodeObject(handle2.DangerousGetHandle(), destination, out decodedValue, out cbDecodedValue);
                    handle2.Dispose();
                    if (flag)
                    {
                        CAPIBase.CERT_ALT_NAME_INFO cert_alt_name_info = (CAPIBase.CERT_ALT_NAME_INFO) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CERT_ALT_NAME_INFO));
                        for (int j = 0; j < cert_alt_name_info.cAltEntry; j++)
                        {
                            IntPtr ptr = new IntPtr(((long) cert_alt_name_info.rgAltEntry) + (j * Marshal.SizeOf(typeof(CAPIBase.CERT_ALT_NAME_ENTRY))));
                            CAPIBase.CERT_ALT_NAME_ENTRY cert_alt_name_entry = (CAPIBase.CERT_ALT_NAME_ENTRY) Marshal.PtrToStructure(ptr, typeof(CAPIBase.CERT_ALT_NAME_ENTRY));
                            switch (dwDisplayType)
                            {
                                case 6:
                                    if (cert_alt_name_entry.dwAltNameChoice == 3)
                                    {
                                        str = Marshal.PtrToStringUni(cert_alt_name_entry.Value.pwszDNSName);
                                    }
                                    break;

                                case 7:
                                    if (cert_alt_name_entry.dwAltNameChoice == 7)
                                    {
                                        str = Marshal.PtrToStringUni(cert_alt_name_entry.Value.pwszURL);
                                    }
                                    break;

                                case 8:
                                    if (cert_alt_name_entry.dwAltNameChoice == 1)
                                    {
                                        CAPIBase.CERT_OTHER_NAME cert_other_name = (CAPIBase.CERT_OTHER_NAME) Marshal.PtrToStructure(cert_alt_name_entry.Value.pOtherName, typeof(CAPIBase.CERT_OTHER_NAME));
                                        if (cert_other_name.pszObjId == "1.3.6.1.4.1.311.20.2.3")
                                        {
                                            uint num6 = 0;
                                            SafeLocalAllocHandle handle3 = null;
                                            if (CAPI.DecodeObject(new IntPtr(0x18L), System.Security.Cryptography.X509Certificates.X509Utils.PtrToByte(cert_other_name.Value.pbData, cert_other_name.Value.cbData), out handle3, out num6))
                                            {
                                                CAPIBase.CERT_NAME_VALUE cert_name_value = (CAPIBase.CERT_NAME_VALUE) Marshal.PtrToStructure(handle3.DangerousGetHandle(), typeof(CAPIBase.CERT_NAME_VALUE));
                                                if (System.Security.Cryptography.X509Certificates.X509Utils.IsCertRdnCharString(cert_name_value.dwValueType))
                                                {
                                                    str = Marshal.PtrToStringUni(cert_name_value.Value.pbData);
                                                }
                                                handle3.Dispose();
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        decodedValue.Dispose();
                    }
                }
            }
            if ((nameType != X509NameType.DnsName) || ((str != null) && (str.Length != 0)))
            {
                return str;
            }
            return CAPI.GetCertNameInfo(this.m_safeCertContext, dwFlags, 3);
        }

        internal static bool GetPrivateKeyInfo(System.Security.Cryptography.SafeCertContextHandle safeCertContext, ref CspParameters parameters)
        {
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            uint pcbData = 0;
            if (!CAPISafe.CertGetCertificateContextProperty(safeCertContext, 2, invalidHandle, ref pcbData))
            {
                if (Marshal.GetLastWin32Error() != -2146885628)
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                return false;
            }
            invalidHandle = CAPI.LocalAlloc(0, new IntPtr((long) pcbData));
            if (!CAPISafe.CertGetCertificateContextProperty(safeCertContext, 2, invalidHandle, ref pcbData))
            {
                if (Marshal.GetLastWin32Error() != -2146885628)
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                return false;
            }
            CAPIBase.CRYPT_KEY_PROV_INFO crypt_key_prov_info = (CAPIBase.CRYPT_KEY_PROV_INFO) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(CAPIBase.CRYPT_KEY_PROV_INFO));
            parameters.ProviderName = crypt_key_prov_info.pwszProvName;
            parameters.KeyContainerName = crypt_key_prov_info.pwszContainerName;
            parameters.ProviderType = (int) crypt_key_prov_info.dwProvType;
            parameters.KeyNumber = (int) crypt_key_prov_info.dwKeySpec;
            parameters.Flags = ((crypt_key_prov_info.dwFlags & 0x20) == 0x20) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags;
            invalidHandle.Dispose();
            return true;
        }

        private static unsafe Oid GetSignatureAlgorithm(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle)
        {
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
            return new Oid(cert_info.SignatureAlgorithm.pszObjId, System.Security.Cryptography.OidGroup.SignatureAlgorithm, false);
        }

        private static unsafe uint GetVersion(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle)
        {
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
            return (cert_info.dwVersion + 1);
        }

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Import(byte[] rawData)
        {
            this.Reset();
            base.Import(rawData);
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Import(string fileName)
        {
            this.Reset();
            base.Import(fileName);
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            base.Import(rawData, password, keyStorageFlags);
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            base.Import(fileName, password, keyStorageFlags);
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            base.Import(rawData, password, keyStorageFlags);
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            base.Import(fileName, password, keyStorageFlags);
            this.m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
        }

        private static unsafe uint QueryCertBlobType(byte[] rawData)
        {
            uint num = 0;
            if (!CAPI.CryptQueryObject(2, rawData, 0x3ffe, 14, 0, IntPtr.Zero, new IntPtr((void*) &num), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return num;
        }

        private static unsafe uint QueryCertFileType(string fileName)
        {
            uint num = 0;
            if (!CAPI.CryptQueryObject(1, fileName, 0x3ffe, 14, 0, IntPtr.Zero, new IntPtr((void*) &num), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return num;
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void Reset()
        {
            this.m_version = 0;
            this.m_notBefore = DateTime.MinValue;
            this.m_notAfter = DateTime.MinValue;
            this.m_privateKey = null;
            this.m_publicKey = null;
            this.m_extensions = null;
            this.m_signatureAlgorithm = null;
            this.m_subjectName = null;
            this.m_issuerName = null;
            if (!this.m_safeCertContext.IsInvalid)
            {
                this.m_safeCertContext.Dispose();
                this.m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            }
            base.Reset();
        }

        private static unsafe void SetFriendlyNameExtendedProperty(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, string name)
        {
            SafeLocalAllocHandle handle = System.Security.Cryptography.X509Certificates.X509Utils.StringToUniPtr(name);
            using (handle)
            {
                CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob = new CAPIBase.CRYPTOAPI_BLOB {
                    cbData = (uint) (2 * (name.Length + 1)),
                    pbData = handle.DangerousGetHandle()
                };
                if (!CAPI.CertSetCertificateContextProperty(safeCertContextHandle, 11, 0, new IntPtr((void*) &cryptoapi_blob)))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
        }

        private static void SetPrivateKeyProperty(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, ICspAsymmetricAlgorithm asymmetricAlgorithm)
        {
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (asymmetricAlgorithm != null)
            {
                CAPIBase.CRYPT_KEY_PROV_INFO structure = new CAPIBase.CRYPT_KEY_PROV_INFO {
                    pwszContainerName = asymmetricAlgorithm.CspKeyContainerInfo.KeyContainerName,
                    pwszProvName = asymmetricAlgorithm.CspKeyContainerInfo.ProviderName,
                    dwProvType = (uint) asymmetricAlgorithm.CspKeyContainerInfo.ProviderType,
                    dwFlags = asymmetricAlgorithm.CspKeyContainerInfo.MachineKeyStore ? 0x20 : 0,
                    cProvParam = 0,
                    rgProvParam = IntPtr.Zero,
                    dwKeySpec = (uint) asymmetricAlgorithm.CspKeyContainerInfo.KeyNumber
                };
                invalidHandle = CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(CAPIBase.CRYPT_KEY_PROV_INFO))));
                Marshal.StructureToPtr(structure, invalidHandle.DangerousGetHandle(), false);
            }
            try
            {
                if (!CAPI.CertSetCertificateContextProperty(safeCertContextHandle, 2, 0, invalidHandle))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (!invalidHandle.IsInvalid)
                {
                    Marshal.DestroyStructure(invalidHandle.DangerousGetHandle(), typeof(CAPIBase.CRYPT_KEY_PROV_INFO));
                    invalidHandle.Dispose();
                }
            }
        }

        public override string ToString()
        {
            return base.ToString(true);
        }

        public override string ToString(bool verbose)
        {
            if (!verbose || this.m_safeCertContext.IsInvalid)
            {
                return this.ToString();
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("[Version]" + Environment.NewLine + "  ");
            sb.Append("V" + this.Version);
            sb.Append(Environment.NewLine + Environment.NewLine + "[Subject]" + Environment.NewLine + "  ");
            sb.Append(this.SubjectName.Name);
            string nameInfo = this.GetNameInfo(X509NameType.SimpleName, false);
            if (nameInfo.Length > 0)
            {
                sb.Append(Environment.NewLine + "  Simple Name: ");
                sb.Append(nameInfo);
            }
            string str2 = this.GetNameInfo(X509NameType.EmailName, false);
            if (str2.Length > 0)
            {
                sb.Append(Environment.NewLine + "  Email Name: ");
                sb.Append(str2);
            }
            string str3 = this.GetNameInfo(X509NameType.UpnName, false);
            if (str3.Length > 0)
            {
                sb.Append(Environment.NewLine + "  UPN Name: ");
                sb.Append(str3);
            }
            string str4 = this.GetNameInfo(X509NameType.DnsName, false);
            if (str4.Length > 0)
            {
                sb.Append(Environment.NewLine + "  DNS Name: ");
                sb.Append(str4);
            }
            sb.Append(Environment.NewLine + Environment.NewLine + "[Issuer]" + Environment.NewLine + "  ");
            sb.Append(this.IssuerName.Name);
            nameInfo = this.GetNameInfo(X509NameType.SimpleName, true);
            if (nameInfo.Length > 0)
            {
                sb.Append(Environment.NewLine + "  Simple Name: ");
                sb.Append(nameInfo);
            }
            str2 = this.GetNameInfo(X509NameType.EmailName, true);
            if (str2.Length > 0)
            {
                sb.Append(Environment.NewLine + "  Email Name: ");
                sb.Append(str2);
            }
            str3 = this.GetNameInfo(X509NameType.UpnName, true);
            if (str3.Length > 0)
            {
                sb.Append(Environment.NewLine + "  UPN Name: ");
                sb.Append(str3);
            }
            str4 = this.GetNameInfo(X509NameType.DnsName, true);
            if (str4.Length > 0)
            {
                sb.Append(Environment.NewLine + "  DNS Name: ");
                sb.Append(str4);
            }
            sb.Append(Environment.NewLine + Environment.NewLine + "[Serial Number]" + Environment.NewLine + "  ");
            sb.Append(this.SerialNumber);
            sb.Append(Environment.NewLine + Environment.NewLine + "[Not Before]" + Environment.NewLine + "  ");
            sb.Append(X509Certificate.FormatDate(this.NotBefore));
            sb.Append(Environment.NewLine + Environment.NewLine + "[Not After]" + Environment.NewLine + "  ");
            sb.Append(X509Certificate.FormatDate(this.NotAfter));
            sb.Append(Environment.NewLine + Environment.NewLine + "[Thumbprint]" + Environment.NewLine + "  ");
            sb.Append(this.Thumbprint);
            sb.Append(Environment.NewLine + Environment.NewLine + "[Signature Algorithm]" + Environment.NewLine + "  ");
            sb.Append(this.SignatureAlgorithm.FriendlyName + "(" + this.SignatureAlgorithm.Value + ")");
            System.Security.Cryptography.X509Certificates.PublicKey publicKey = this.PublicKey;
            sb.Append(Environment.NewLine + Environment.NewLine + "[Public Key]" + Environment.NewLine + "  Algorithm: ");
            sb.Append(publicKey.Oid.FriendlyName);
            sb.Append(Environment.NewLine + "  Length: ");
            sb.Append(publicKey.Key.KeySize);
            sb.Append(Environment.NewLine + "  Key Blob: ");
            sb.Append(publicKey.EncodedKeyValue.Format(true));
            sb.Append(Environment.NewLine + "  Parameters: ");
            sb.Append(publicKey.EncodedParameters.Format(true));
            this.AppendPrivateKeyInfo(sb);
            X509ExtensionCollection extensions = this.Extensions;
            if (extensions.Count > 0)
            {
                sb.Append(Environment.NewLine + Environment.NewLine + "[Extensions]");
                X509ExtensionEnumerator enumerator = extensions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Extension current = enumerator.Current;
                    sb.Append(Environment.NewLine + "* " + current.Oid.FriendlyName + "(" + current.Oid.Value + "):" + Environment.NewLine + "  " + current.Format(true));
                }
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        public bool Verify()
        {
            if (this.m_safeCertContext.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
            }
            return (System.Security.Cryptography.X509Certificates.X509Utils.VerifyCertificate(this.CertContext, null, null, X509RevocationMode.Online, X509RevocationFlag.ExcludeRoot, DateTime.Now, new TimeSpan(0, 0, 0), null, new IntPtr(1L), IntPtr.Zero) == 0);
        }

        public bool Archived
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                uint pcbData = 0;
                return CAPISafe.CertGetCertificateContextProperty(this.m_safeCertContext, 0x13, SafeLocalAllocHandle.InvalidHandle, ref pcbData);
            }
            set
            {
                SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
                if (value)
                {
                    invalidHandle = CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(CAPIBase.CRYPTOAPI_BLOB))));
                }
                if (!CAPI.CertSetCertificateContextProperty(this.m_safeCertContext, 0x13, 0, invalidHandle))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                invalidHandle.Dispose();
            }
        }

        internal System.Security.Cryptography.SafeCertContextHandle CertContext
        {
            get
            {
                return this.m_safeCertContext;
            }
        }

        public X509ExtensionCollection Extensions
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_extensions == null)
                {
                    this.m_extensions = new X509ExtensionCollection(this.m_safeCertContext);
                }
                return this.m_extensions;
            }
        }

        public string FriendlyName
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
                uint pcbData = 0;
                if (!CAPISafe.CertGetCertificateContextProperty(this.m_safeCertContext, 11, invalidHandle, ref pcbData))
                {
                    return string.Empty;
                }
                invalidHandle = CAPI.LocalAlloc(0, new IntPtr((long) pcbData));
                if (!CAPISafe.CertGetCertificateContextProperty(this.m_safeCertContext, 11, invalidHandle, ref pcbData))
                {
                    return string.Empty;
                }
                string str = Marshal.PtrToStringUni(invalidHandle.DangerousGetHandle());
                invalidHandle.Dispose();
                return str;
            }
            set
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (value == null)
                {
                    value = string.Empty;
                }
                SetFriendlyNameExtendedProperty(this.m_safeCertContext, value);
            }
        }

        public bool HasPrivateKey
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                uint pcbData = 0;
                return CAPISafe.CertGetCertificateContextProperty(this.m_safeCertContext, 2, SafeLocalAllocHandle.InvalidHandle, ref pcbData);
            }
        }

        public X500DistinguishedName IssuerName
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_issuerName == null)
                {
                    CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) this.m_safeCertContext.DangerousGetHandle());
                    CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
                    this.m_issuerName = new X500DistinguishedName(cert_info.Issuer);
                }
                return this.m_issuerName;
            }
        }

        public DateTime NotAfter
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_notAfter == DateTime.MinValue)
                {
                    CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) this.m_safeCertContext.DangerousGetHandle());
                    CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
                    long fileTime = (long) ((((ulong) cert_info.NotAfter.dwHighDateTime) << 0x20) | ((ulong) cert_info.NotAfter.dwLowDateTime));
                    this.m_notAfter = DateTime.FromFileTime(fileTime);
                }
                return this.m_notAfter;
            }
        }

        public DateTime NotBefore
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_notBefore == DateTime.MinValue)
                {
                    CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) this.m_safeCertContext.DangerousGetHandle());
                    CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
                    long fileTime = (long) ((((ulong) cert_info.NotBefore.dwHighDateTime) << 0x20) | ((ulong) cert_info.NotBefore.dwLowDateTime));
                    this.m_notBefore = DateTime.FromFileTime(fileTime);
                }
                return this.m_notBefore;
            }
        }

        public AsymmetricAlgorithm PrivateKey
        {
            get
            {
                if (!this.HasPrivateKey)
                {
                    return null;
                }
                if (this.m_privateKey == null)
                {
                    CspParameters parameters = new CspParameters();
                    if (!GetPrivateKeyInfo(this.m_safeCertContext, ref parameters))
                    {
                        return null;
                    }
                    parameters.Flags |= CspProviderFlags.UseExistingKey;
                    uint algorithmId = this.PublicKey.AlgorithmId;
                    if (algorithmId != 0x2200)
                    {
                        if ((algorithmId != 0x2400) && (algorithmId != 0xa400))
                        {
                            throw new NotSupportedException(SR.GetString("NotSupported_KeyAlgorithm"));
                        }
                        this.m_privateKey = new RSACryptoServiceProvider(parameters);
                    }
                    else
                    {
                        this.m_privateKey = new DSACryptoServiceProvider(parameters);
                    }
                }
                return this.m_privateKey;
            }
            set
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                ICspAsymmetricAlgorithm asymmetricAlgorithm = value as ICspAsymmetricAlgorithm;
                if ((value != null) && (asymmetricAlgorithm == null))
                {
                    throw new NotSupportedException(SR.GetString("NotSupported_InvalidKeyImpl"));
                }
                if (asymmetricAlgorithm != null)
                {
                    if (asymmetricAlgorithm.CspKeyContainerInfo == null)
                    {
                        throw new ArgumentException("CspKeyContainerInfo");
                    }
                    if (s_publicKeyOffset == 0)
                    {
                        s_publicKeyOffset = Marshal.SizeOf(typeof(CAPIBase.BLOBHEADER));
                    }
                    byte[] buffer = (this.PublicKey.Key as ICspAsymmetricAlgorithm).ExportCspBlob(false);
                    byte[] buffer2 = asymmetricAlgorithm.ExportCspBlob(false);
                    if (((buffer == null) || (buffer2 == null)) || ((buffer.Length != buffer2.Length) || (buffer.Length <= s_publicKeyOffset)))
                    {
                        throw new CryptographicUnexpectedOperationException(SR.GetString("Cryptography_X509_KeyMismatch"));
                    }
                    for (int i = s_publicKeyOffset; i < buffer.Length; i++)
                    {
                        if (buffer[i] != buffer2[i])
                        {
                            throw new CryptographicUnexpectedOperationException(SR.GetString("Cryptography_X509_KeyMismatch"));
                        }
                    }
                }
                SetPrivateKeyProperty(this.m_safeCertContext, asymmetricAlgorithm);
                this.m_privateKey = value;
            }
        }

        public System.Security.Cryptography.X509Certificates.PublicKey PublicKey
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_publicKey == null)
                {
                    string keyAlgorithm = this.GetKeyAlgorithm();
                    byte[] keyAlgorithmParameters = this.GetKeyAlgorithmParameters();
                    byte[] publicKey = this.GetPublicKey();
                    Oid oid = new Oid(keyAlgorithm, System.Security.Cryptography.OidGroup.PublicKeyAlgorithm, true);
                    this.m_publicKey = new System.Security.Cryptography.X509Certificates.PublicKey(oid, new AsnEncodedData(oid, keyAlgorithmParameters), new AsnEncodedData(oid, publicKey));
                }
                return this.m_publicKey;
            }
        }

        public byte[] RawData
        {
            get
            {
                return this.GetRawCertData();
            }
        }

        public string SerialNumber
        {
            get
            {
                return this.GetSerialNumberString();
            }
        }

        public Oid SignatureAlgorithm
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_signatureAlgorithm == null)
                {
                    this.m_signatureAlgorithm = GetSignatureAlgorithm(this.m_safeCertContext);
                }
                return this.m_signatureAlgorithm;
            }
        }

        public X500DistinguishedName SubjectName
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_subjectName == null)
                {
                    CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) this.m_safeCertContext.DangerousGetHandle());
                    CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
                    this.m_subjectName = new X500DistinguishedName(cert_info.Subject);
                }
                return this.m_subjectName;
            }
        }

        public string Thumbprint
        {
            get
            {
                return this.GetCertHashString();
            }
        }

        public int Version
        {
            get
            {
                if (this.m_safeCertContext.IsInvalid)
                {
                    throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
                }
                if (this.m_version == 0)
                {
                    this.m_version = (int) GetVersion(this.m_safeCertContext);
                }
                return this.m_version;
            }
        }
    }
}

