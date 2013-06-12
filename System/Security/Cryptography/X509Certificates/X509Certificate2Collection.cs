namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    public class X509Certificate2Collection : X509CertificateCollection
    {
        private const uint X509_STORE_CONTENT_FLAGS = 0x1732;

        public X509Certificate2Collection()
        {
        }

        public X509Certificate2Collection(X509Certificate2 certificate)
        {
            this.Add(certificate);
        }

        public X509Certificate2Collection(X509Certificate2Collection certificates)
        {
            this.AddRange(certificates);
        }

        public X509Certificate2Collection(X509Certificate2[] certificates)
        {
            this.AddRange(certificates);
        }

        public int Add(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            return base.List.Add(certificate);
        }

        public void AddRange(X509Certificate2[] certificates)
        {
            if (certificates == null)
            {
                throw new ArgumentNullException("certificates");
            }
            int index = 0;
            try
            {
                while (index < certificates.Length)
                {
                    this.Add(certificates[index]);
                    index++;
                }
            }
            catch
            {
                for (int i = 0; i < index; i++)
                {
                    this.Remove(certificates[i]);
                }
                throw;
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

        public bool Contains(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            return base.List.Contains(certificate);
        }

        public byte[] Export(X509ContentType contentType)
        {
            return this.Export(contentType, null);
        }

        public byte[] Export(X509ContentType contentType, string password)
        {
            new StorePermission(StorePermissionFlags.AllFlags).Assert();
            System.Security.Cryptography.SafeCertStoreHandle safeCertStoreHandle = System.Security.Cryptography.X509Certificates.X509Utils.ExportToMemoryStore(this);
            byte[] buffer = ExportCertificatesToBlob(safeCertStoreHandle, contentType, password);
            safeCertStoreHandle.Dispose();
            return buffer;
        }

        private static unsafe byte[] ExportCertificatesToBlob(System.Security.Cryptography.SafeCertStoreHandle safeCertStoreHandle, X509ContentType contentType, string password)
        {
            System.Security.Cryptography.SafeCertContextHandle invalidHandle = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            uint dwSaveAs = 2;
            byte[] destination = null;
            CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob = new CAPIBase.CRYPTOAPI_BLOB();
            SafeLocalAllocHandle pbElement = SafeLocalAllocHandle.InvalidHandle;
            switch (contentType)
            {
                case X509ContentType.Cert:
                    invalidHandle = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, invalidHandle);
                    if ((invalidHandle != null) && !invalidHandle.IsInvalid)
                    {
                        CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) invalidHandle.DangerousGetHandle());
                        destination = new byte[cert_context.cbCertEncoded];
                        Marshal.Copy(cert_context.pbCertEncoded, destination, 0, destination.Length);
                    }
                    break;

                case X509ContentType.SerializedCert:
                {
                    invalidHandle = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, invalidHandle);
                    uint num2 = 0;
                    if ((invalidHandle != null) && !invalidHandle.IsInvalid)
                    {
                        if (!CAPISafe.CertSerializeCertificateStoreElement(invalidHandle, 0, pbElement, new IntPtr((void*) &num2)))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        pbElement = CAPI.LocalAlloc(0, new IntPtr((long) num2));
                        if (!CAPISafe.CertSerializeCertificateStoreElement(invalidHandle, 0, pbElement, new IntPtr((void*) &num2)))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        destination = new byte[num2];
                        Marshal.Copy(pbElement.DangerousGetHandle(), destination, 0, destination.Length);
                        break;
                    }
                    break;
                }
                case X509ContentType.Pfx:
                    if (!CAPI.PFXExportCertStore(safeCertStoreHandle, new IntPtr((void*) &cryptoapi_blob), password, 6))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    cryptoapi_blob.pbData = CAPI.LocalAlloc(0, new IntPtr((long) cryptoapi_blob.cbData)).DangerousGetHandle();
                    if (!CAPI.PFXExportCertStore(safeCertStoreHandle, new IntPtr((void*) &cryptoapi_blob), password, 6))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    destination = new byte[cryptoapi_blob.cbData];
                    Marshal.Copy(cryptoapi_blob.pbData, destination, 0, destination.Length);
                    break;

                case X509ContentType.SerializedStore:
                case X509ContentType.Pkcs7:
                    if (contentType == X509ContentType.SerializedStore)
                    {
                        dwSaveAs = 1;
                    }
                    if (!CAPI.CertSaveStore(safeCertStoreHandle, 0x10001, dwSaveAs, 2, new IntPtr((void*) &cryptoapi_blob), 0))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    pbElement = CAPI.LocalAlloc(0, new IntPtr((long) cryptoapi_blob.cbData));
                    cryptoapi_blob.pbData = pbElement.DangerousGetHandle();
                    if (!CAPI.CertSaveStore(safeCertStoreHandle, 0x10001, dwSaveAs, 2, new IntPtr((void*) &cryptoapi_blob), 0))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    destination = new byte[cryptoapi_blob.cbData];
                    Marshal.Copy(cryptoapi_blob.pbData, destination, 0, destination.Length);
                    break;

                default:
                    throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidContentType"));
            }
            pbElement.Dispose();
            invalidHandle.Dispose();
            return destination;
        }

        public X509Certificate2Collection Find(X509FindType findType, object findValue, bool validOnly)
        {
            new StorePermission(StorePermissionFlags.AllFlags).Assert();
            System.Security.Cryptography.SafeCertStoreHandle safeSourceStoreHandle = System.Security.Cryptography.X509Certificates.X509Utils.ExportToMemoryStore(this);
            System.Security.Cryptography.SafeCertStoreHandle safeCertStoreHandle = FindCertInStore(safeSourceStoreHandle, findType, findValue, validOnly);
            X509Certificate2Collection certificates = System.Security.Cryptography.X509Certificates.X509Utils.GetCertificates(safeCertStoreHandle);
            safeCertStoreHandle.Dispose();
            safeSourceStoreHandle.Dispose();
            return certificates;
        }

        private static unsafe int FindApplicationPolicyCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            string strA = (string) pvCallbackData;
            if (strA.Length != 0)
            {
                IntPtr ptr = safeCertContextHandle.DangerousGetHandle();
                int num = 0;
                uint num2 = 0;
                SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
                if (!CAPISafe.CertGetValidUsages(1, new IntPtr((void*) &ptr), new IntPtr((void*) &num), invalidHandle, new IntPtr((void*) &num2)))
                {
                    return 1;
                }
                invalidHandle = CAPI.LocalAlloc(0, new IntPtr((long) num2));
                if (!CAPISafe.CertGetValidUsages(1, new IntPtr((void*) &ptr), new IntPtr((void*) &num), invalidHandle, new IntPtr((void*) &num2)))
                {
                    return 1;
                }
                if (num == -1)
                {
                    return 0;
                }
                for (int i = 0; i < num; i++)
                {
                    string strB = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(new IntPtr(((long) invalidHandle.DangerousGetHandle()) + (i * Marshal.SizeOf(typeof(IntPtr))))));
                    if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return 0;
                    }
                }
            }
            return 1;
        }

        private static void FindByCert(System.Security.Cryptography.SafeCertStoreHandle safeSourceStoreHandle, uint dwFindType, IntPtr pvFindPara, bool validOnly, FindProcDelegate pfnCertCallback1, FindProcDelegate pfnCertCallback2, object pvCallbackData1, object pvCallbackData2, System.Security.Cryptography.SafeCertStoreHandle safeTargetStoreHandle)
        {
            int hr = 0;
            System.Security.Cryptography.SafeCertContextHandle invalidHandle = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            invalidHandle = CAPI.CertFindCertificateInStore(safeSourceStoreHandle, 0x10001, 0, dwFindType, pvFindPara, invalidHandle);
            while ((invalidHandle != null) && !invalidHandle.IsInvalid)
            {
                if (pfnCertCallback1 != null)
                {
                    hr = pfnCertCallback1(invalidHandle, pvCallbackData1);
                    if (hr == 1)
                    {
                        if (pfnCertCallback2 != null)
                        {
                            hr = pfnCertCallback2(invalidHandle, pvCallbackData2);
                        }
                        if (hr == 1)
                        {
                            goto Label_008D;
                        }
                    }
                    if (hr != 0)
                    {
                        break;
                    }
                }
                if (validOnly)
                {
                    hr = System.Security.Cryptography.X509Certificates.X509Utils.VerifyCertificate(invalidHandle, null, null, X509RevocationMode.NoCheck, X509RevocationFlag.ExcludeRoot, DateTime.Now, new TimeSpan(0, 0, 0), null, new IntPtr(1L), IntPtr.Zero);
                    if (hr == 1)
                    {
                        goto Label_008D;
                    }
                    if (hr != 0)
                    {
                        break;
                    }
                }
                if (!CAPI.CertAddCertificateLinkToStore(safeTargetStoreHandle, invalidHandle, 4, System.Security.Cryptography.SafeCertContextHandle.InvalidHandle))
                {
                    hr = Marshal.GetHRForLastWin32Error();
                    break;
                }
            Label_008D:
                GC.SuppressFinalize(invalidHandle);
                invalidHandle = CAPI.CertFindCertificateInStore(safeSourceStoreHandle, 0x10001, 0, dwFindType, pvFindPara, invalidHandle);
            }
            if ((invalidHandle != null) && !invalidHandle.IsInvalid)
            {
                invalidHandle.Dispose();
            }
            if ((hr != 1) && (hr != 0))
            {
                throw new CryptographicException(hr);
            }
        }

        private static unsafe int FindCertificatePolicyCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            string strA = (string) pvCallbackData;
            if (strA.Length != 0)
            {
                CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
                CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
                IntPtr ptr = CAPISafe.CertFindExtension("2.5.29.32", cert_info.cExtension, cert_info.rgExtension);
                if (ptr == IntPtr.Zero)
                {
                    return 1;
                }
                CAPIBase.CERT_EXTENSION cert_extension = (CAPIBase.CERT_EXTENSION) Marshal.PtrToStructure(ptr, typeof(CAPIBase.CERT_EXTENSION));
                byte[] destination = new byte[cert_extension.Value.cbData];
                Marshal.Copy(cert_extension.Value.pbData, destination, 0, destination.Length);
                uint cbDecodedValue = 0;
                SafeLocalAllocHandle decodedValue = null;
                if (CAPI.DecodeObject(new IntPtr(0x10L), destination, out decodedValue, out cbDecodedValue))
                {
                    CAPIBase.CERT_POLICIES_INFO cert_policies_info = (CAPIBase.CERT_POLICIES_INFO) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CERT_POLICIES_INFO));
                    for (int i = 0; i < cert_policies_info.cPolicyInfo; i++)
                    {
                        IntPtr ptr2 = new IntPtr(((long) cert_policies_info.rgPolicyInfo) + (i * Marshal.SizeOf(typeof(CAPIBase.CERT_POLICY_INFO))));
                        CAPIBase.CERT_POLICY_INFO cert_policy_info = (CAPIBase.CERT_POLICY_INFO) Marshal.PtrToStructure(ptr2, typeof(CAPIBase.CERT_POLICY_INFO));
                        if (string.Compare(strA, cert_policy_info.pszPolicyIdentifier, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return 0;
                        }
                    }
                }
            }
            return 1;
        }

        private static unsafe System.Security.Cryptography.SafeCertStoreHandle FindCertInStore(System.Security.Cryptography.SafeCertStoreHandle safeSourceStoreHandle, X509FindType findType, object findValue, bool validOnly)
        {
            string str;
            string str2;
            System.Security.Cryptography.SafeCertStoreHandle handle2;
            if (findValue == null)
            {
                throw new ArgumentNullException("findValue");
            }
            IntPtr zero = IntPtr.Zero;
            object dwKeyUsageBit = null;
            object obj3 = null;
            FindProcDelegate delegate2 = null;
            FindProcDelegate delegate3 = null;
            uint dwFindType = 0;
            CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob = new CAPIBase.CRYPTOAPI_BLOB();
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            System.Runtime.InteropServices.ComTypes.FILETIME filetime = new System.Runtime.InteropServices.ComTypes.FILETIME();
            string keyValue = null;
            switch (findType)
            {
                case X509FindType.FindByThumbprint:
                {
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    byte[] managed = System.Security.Cryptography.X509Certificates.X509Utils.DecodeHexString((string) findValue);
                    cryptoapi_blob.pbData = System.Security.Cryptography.X509Certificates.X509Utils.ByteToPtr(managed).DangerousGetHandle();
                    cryptoapi_blob.cbData = (uint) managed.Length;
                    dwFindType = 0x10000;
                    zero = new IntPtr((void*) &cryptoapi_blob);
                    goto Label_0703;
                }
                case X509FindType.FindBySubjectName:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    str = (string) findValue;
                    dwFindType = 0x80007;
                    zero = System.Security.Cryptography.X509Certificates.X509Utils.StringToUniPtr(str).DangerousGetHandle();
                    goto Label_0703;

                case X509FindType.FindBySubjectDistinguishedName:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    str = (string) findValue;
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindSubjectDistinguishedNameCallback);
                    dwKeyUsageBit = str;
                    goto Label_0703;

                case X509FindType.FindByIssuerName:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    str2 = (string) findValue;
                    dwFindType = 0x80004;
                    invalidHandle = System.Security.Cryptography.X509Certificates.X509Utils.StringToUniPtr(str2);
                    zero = invalidHandle.DangerousGetHandle();
                    goto Label_0703;

                case X509FindType.FindByIssuerDistinguishedName:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    str2 = (string) findValue;
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindIssuerDistinguishedNameCallback);
                    dwKeyUsageBit = str2;
                    goto Label_0703;

                case X509FindType.FindBySerialNumber:
                {
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindSerialNumberCallback);
                    delegate3 = new FindProcDelegate(X509Certificate2Collection.FindSerialNumberCallback);
                    BigInt num2 = new BigInt();
                    num2.FromHexadecimal((string) findValue);
                    dwKeyUsageBit = num2.ToByteArray();
                    num2.FromDecimal((string) findValue);
                    obj3 = num2.ToByteArray();
                    goto Label_0703;
                }
                case X509FindType.FindByTimeValid:
                    if (findValue.GetType() != typeof(DateTime))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    *((long*) &filetime) = ((DateTime) findValue).ToFileTime();
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindTimeValidCallback);
                    dwKeyUsageBit = filetime;
                    goto Label_0703;

                case X509FindType.FindByTimeNotYetValid:
                    if (findValue.GetType() != typeof(DateTime))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    *((long*) &filetime) = ((DateTime) findValue).ToFileTime();
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindTimeNotBeforeCallback);
                    dwKeyUsageBit = filetime;
                    goto Label_0703;

                case X509FindType.FindByTimeExpired:
                    if (findValue.GetType() != typeof(DateTime))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    *((long*) &filetime) = ((DateTime) findValue).ToFileTime();
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindTimeNotAfterCallback);
                    dwKeyUsageBit = filetime;
                    goto Label_0703;

                case X509FindType.FindByTemplateName:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    dwKeyUsageBit = (string) findValue;
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindTemplateNameCallback);
                    goto Label_0703;

                case X509FindType.FindByApplicationPolicy:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    keyValue = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, (string) findValue, System.Security.Cryptography.OidGroup.Policy);
                    if (keyValue == null)
                    {
                        keyValue = (string) findValue;
                        System.Security.Cryptography.X509Certificates.X509Utils.ValidateOidValue(keyValue);
                    }
                    dwKeyUsageBit = keyValue;
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindApplicationPolicyCallback);
                    goto Label_0703;

                case X509FindType.FindByCertificatePolicy:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    keyValue = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, (string) findValue, System.Security.Cryptography.OidGroup.Policy);
                    if (keyValue == null)
                    {
                        keyValue = (string) findValue;
                        System.Security.Cryptography.X509Certificates.X509Utils.ValidateOidValue(keyValue);
                    }
                    dwKeyUsageBit = keyValue;
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindCertificatePolicyCallback);
                    goto Label_0703;

                case X509FindType.FindByExtension:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    keyValue = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, (string) findValue, System.Security.Cryptography.OidGroup.ExtensionOrAttribute);
                    if (keyValue == null)
                    {
                        keyValue = (string) findValue;
                        System.Security.Cryptography.X509Certificates.X509Utils.ValidateOidValue(keyValue);
                    }
                    dwKeyUsageBit = keyValue;
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindExtensionCallback);
                    goto Label_0703;

                case X509FindType.FindByKeyUsage:
                {
                    if (!(findValue.GetType() == typeof(string)))
                    {
                        if (findValue.GetType() == typeof(X509KeyUsageFlags))
                        {
                            dwKeyUsageBit = findValue;
                        }
                        else
                        {
                            if (!(findValue.GetType() == typeof(uint)) && !(findValue.GetType() == typeof(int)))
                            {
                                throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindType"));
                            }
                            dwKeyUsageBit = findValue;
                        }
                        goto Label_06A2;
                    }
                    CAPIBase.KEY_USAGE_STRUCT[] key_usage_structArray = new CAPIBase.KEY_USAGE_STRUCT[] { new CAPIBase.KEY_USAGE_STRUCT("DigitalSignature", 0x80), new CAPIBase.KEY_USAGE_STRUCT("NonRepudiation", 0x40), new CAPIBase.KEY_USAGE_STRUCT("KeyEncipherment", 0x20), new CAPIBase.KEY_USAGE_STRUCT("DataEncipherment", 0x10), new CAPIBase.KEY_USAGE_STRUCT("KeyAgreement", 8), new CAPIBase.KEY_USAGE_STRUCT("KeyCertSign", 4), new CAPIBase.KEY_USAGE_STRUCT("CrlSign", 2), new CAPIBase.KEY_USAGE_STRUCT("EncipherOnly", 1), new CAPIBase.KEY_USAGE_STRUCT("DecipherOnly", 0x8000) };
                    for (uint i = 0; i < key_usage_structArray.Length; i++)
                    {
                        if (string.Compare(key_usage_structArray[i].pwszKeyUsage, (string) findValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            dwKeyUsageBit = key_usage_structArray[i].dwKeyUsageBit;
                            break;
                        }
                    }
                    break;
                }
                case X509FindType.FindBySubjectKeyIdentifier:
                    if (findValue.GetType() != typeof(string))
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindValue"));
                    }
                    dwKeyUsageBit = System.Security.Cryptography.X509Certificates.X509Utils.DecodeHexString((string) findValue);
                    delegate2 = new FindProcDelegate(X509Certificate2Collection.FindSubjectKeyIdentifierCallback);
                    goto Label_0703;

                default:
                    throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindType"));
            }
            if (dwKeyUsageBit == null)
            {
                throw new CryptographicException(SR.GetString("Cryptography_X509_InvalidFindType"));
            }
        Label_06A2:
            delegate2 = new FindProcDelegate(X509Certificate2Collection.FindKeyUsageCallback);
        Label_0703:
            handle2 = CAPI.CertOpenStore(new IntPtr(2L), 0x10001, IntPtr.Zero, 0x2200, null);
            if ((handle2 == null) || handle2.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            FindByCert(safeSourceStoreHandle, dwFindType, zero, validOnly, delegate2, delegate3, dwKeyUsageBit, obj3, handle2);
            invalidHandle.Dispose();
            return handle2;
        }

        private static unsafe int FindExtensionCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
            if (CAPISafe.CertFindExtension((string) pvCallbackData, cert_info.cExtension, cert_info.rgExtension) == IntPtr.Zero)
            {
                return 1;
            }
            return 0;
        }

        private static int FindIssuerDistinguishedNameCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            if (string.Compare(CAPI.GetCertNameInfo(safeCertContextHandle, 1, 2), (string) pvCallbackData, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return 1;
            }
            return 0;
        }

        private static unsafe int FindKeyUsageCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            uint num = 0;
            if (!CAPISafe.CertGetIntendedKeyUsage(0x10001, cert_context.pCertInfo, new IntPtr((void*) &num), 4))
            {
                return 0;
            }
            uint num2 = Convert.ToUInt32(pvCallbackData, null);
            if ((num & num2) == num2)
            {
                return 0;
            }
            return 1;
        }

        private static unsafe int FindSerialNumberCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
            byte[] destination = new byte[cert_info.SerialNumber.cbData];
            Marshal.Copy(cert_info.SerialNumber.pbData, destination, 0, destination.Length);
            int hexArraySize = System.Security.Cryptography.X509Certificates.X509Utils.GetHexArraySize(destination);
            byte[] buffer2 = (byte[]) pvCallbackData;
            if (buffer2.Length != hexArraySize)
            {
                return 1;
            }
            for (int i = 0; i < buffer2.Length; i++)
            {
                if (buffer2[i] != destination[i])
                {
                    return 1;
                }
            }
            return 0;
        }

        private static int FindSubjectDistinguishedNameCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            if (string.Compare(CAPI.GetCertNameInfo(safeCertContextHandle, 0, 2), (string) pvCallbackData, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return 1;
            }
            return 0;
        }

        private static int FindSubjectKeyIdentifierCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            uint pcbData = 0;
            if (!CAPISafe.CertGetCertificateContextProperty(safeCertContextHandle, 20, invalidHandle, ref pcbData))
            {
                return 1;
            }
            invalidHandle = CAPI.LocalAlloc(0, new IntPtr((long) pcbData));
            if (!CAPISafe.CertGetCertificateContextProperty(safeCertContextHandle, 20, invalidHandle, ref pcbData))
            {
                return 1;
            }
            byte[] buffer = (byte[]) pvCallbackData;
            if (buffer.Length != pcbData)
            {
                return 1;
            }
            byte[] destination = new byte[pcbData];
            Marshal.Copy(invalidHandle.DangerousGetHandle(), destination, 0, destination.Length);
            invalidHandle.Dispose();
            for (uint i = 0; i < pcbData; i++)
            {
                if (buffer[i] != destination[i])
                {
                    return 1;
                }
            }
            return 0;
        }

        private static unsafe int FindTemplateNameCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
            zero = CAPISafe.CertFindExtension("1.3.6.1.4.1.311.20.2", cert_info.cExtension, cert_info.rgExtension);
            ptr = CAPISafe.CertFindExtension("1.3.6.1.4.1.311.21.7", cert_info.cExtension, cert_info.rgExtension);
            if ((zero != IntPtr.Zero) || (ptr != IntPtr.Zero))
            {
                if (zero != IntPtr.Zero)
                {
                    CAPIBase.CERT_EXTENSION cert_extension = (CAPIBase.CERT_EXTENSION) Marshal.PtrToStructure(zero, typeof(CAPIBase.CERT_EXTENSION));
                    byte[] destination = new byte[cert_extension.Value.cbData];
                    Marshal.Copy(cert_extension.Value.pbData, destination, 0, destination.Length);
                    uint cbDecodedValue = 0;
                    SafeLocalAllocHandle decodedValue = null;
                    if (CAPI.DecodeObject(new IntPtr(0x18L), destination, out decodedValue, out cbDecodedValue))
                    {
                        CAPIBase.CERT_NAME_VALUE cert_name_value = (CAPIBase.CERT_NAME_VALUE) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CERT_NAME_VALUE));
                        if (string.Compare(Marshal.PtrToStringUni(cert_name_value.Value.pbData), (string) pvCallbackData, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return 0;
                        }
                    }
                }
                if (ptr != IntPtr.Zero)
                {
                    CAPIBase.CERT_EXTENSION cert_extension2 = (CAPIBase.CERT_EXTENSION) Marshal.PtrToStructure(ptr, typeof(CAPIBase.CERT_EXTENSION));
                    byte[] buffer2 = new byte[cert_extension2.Value.cbData];
                    Marshal.Copy(cert_extension2.Value.pbData, buffer2, 0, buffer2.Length);
                    uint num2 = 0;
                    SafeLocalAllocHandle handle2 = null;
                    if (CAPI.DecodeObject(new IntPtr(0x40L), buffer2, out handle2, out num2))
                    {
                        CAPIBase.CERT_TEMPLATE_EXT cert_template_ext = (CAPIBase.CERT_TEMPLATE_EXT) Marshal.PtrToStructure(handle2.DangerousGetHandle(), typeof(CAPIBase.CERT_TEMPLATE_EXT));
                        string strB = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, (string) pvCallbackData, System.Security.Cryptography.OidGroup.Template);
                        if (strB == null)
                        {
                            strB = (string) pvCallbackData;
                        }
                        if (string.Compare(cert_template_ext.pszObjId, strB, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return 0;
                        }
                    }
                }
            }
            return 1;
        }

        private static unsafe int FindTimeNotAfterCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            System.Runtime.InteropServices.ComTypes.FILETIME pTimeToVerify = (System.Runtime.InteropServices.ComTypes.FILETIME) pvCallbackData;
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            if (CAPISafe.CertVerifyTimeValidity(ref pTimeToVerify, cert_context.pCertInfo) == 1)
            {
                return 0;
            }
            return 1;
        }

        private static unsafe int FindTimeNotBeforeCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            System.Runtime.InteropServices.ComTypes.FILETIME pTimeToVerify = (System.Runtime.InteropServices.ComTypes.FILETIME) pvCallbackData;
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            if (CAPISafe.CertVerifyTimeValidity(ref pTimeToVerify, cert_context.pCertInfo) == -1)
            {
                return 0;
            }
            return 1;
        }

        private static unsafe int FindTimeValidCallback(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData)
        {
            System.Runtime.InteropServices.ComTypes.FILETIME pTimeToVerify = (System.Runtime.InteropServices.ComTypes.FILETIME) pvCallbackData;
            CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) safeCertContextHandle.DangerousGetHandle());
            if (CAPISafe.CertVerifyTimeValidity(ref pTimeToVerify, cert_context.pCertInfo) == 0)
            {
                return 0;
            }
            return 1;
        }

        public X509Certificate2Enumerator GetEnumerator()
        {
            return new X509Certificate2Enumerator(this);
        }

        public void Import(byte[] rawData)
        {
            this.Import(rawData, null, X509KeyStorageFlags.DefaultKeySet);
        }

        public void Import(string fileName)
        {
            this.Import(fileName, null, X509KeyStorageFlags.DefaultKeySet);
        }

        public void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
        {
            uint dwFlags = System.Security.Cryptography.X509Certificates.X509Utils.MapKeyStorageFlags(keyStorageFlags);
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            new StorePermission(StorePermissionFlags.AllFlags).Assert();
            invalidHandle = LoadStoreFromBlob(rawData, password, dwFlags, (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != X509KeyStorageFlags.DefaultKeySet);
            X509Certificate2Collection certificates = System.Security.Cryptography.X509Certificates.X509Utils.GetCertificates(invalidHandle);
            invalidHandle.Dispose();
            X509Certificate2[] array = new X509Certificate2[certificates.Count];
            certificates.CopyTo(array, 0);
            this.AddRange(array);
        }

        public void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
        {
            uint dwFlags = System.Security.Cryptography.X509Certificates.X509Utils.MapKeyStorageFlags(keyStorageFlags);
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            new StorePermission(StorePermissionFlags.AllFlags).Assert();
            invalidHandle = LoadStoreFromFile(fileName, password, dwFlags, (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != X509KeyStorageFlags.DefaultKeySet);
            X509Certificate2Collection certificates = System.Security.Cryptography.X509Certificates.X509Utils.GetCertificates(invalidHandle);
            invalidHandle.Dispose();
            X509Certificate2[] array = new X509Certificate2[certificates.Count];
            certificates.CopyTo(array, 0);
            this.AddRange(array);
        }

        public void Insert(int index, X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            base.List.Insert(index, certificate);
        }

        private static unsafe System.Security.Cryptography.SafeCertStoreHandle LoadStoreFromBlob(byte[] rawData, string password, uint dwFlags, bool persistKeyContainers)
        {
            uint num = 0;
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            if (!CAPI.CryptQueryObject(2, rawData, 0x1732, 14, 0, IntPtr.Zero, new IntPtr((void*) &num), IntPtr.Zero, ref invalidHandle, IntPtr.Zero, IntPtr.Zero))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (num == 12)
            {
                invalidHandle.Dispose();
                invalidHandle = CAPI.PFXImportCertStore(2, rawData, password, dwFlags, persistKeyContainers);
            }
            if ((invalidHandle == null) || invalidHandle.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return invalidHandle;
        }

        private static unsafe System.Security.Cryptography.SafeCertStoreHandle LoadStoreFromFile(string fileName, string password, uint dwFlags, bool persistKeyContainers)
        {
            uint num = 0;
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            if (!CAPI.CryptQueryObject(1, fileName, 0x1732, 14, 0, IntPtr.Zero, new IntPtr((void*) &num), IntPtr.Zero, ref invalidHandle, IntPtr.Zero, IntPtr.Zero))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (num == 12)
            {
                invalidHandle.Dispose();
                invalidHandle = CAPI.PFXImportCertStore(1, fileName, password, dwFlags, persistKeyContainers);
            }
            if ((invalidHandle == null) || invalidHandle.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return invalidHandle;
        }

        public void Remove(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            base.List.Remove(certificate);
        }

        public void RemoveRange(X509Certificate2[] certificates)
        {
            if (certificates == null)
            {
                throw new ArgumentNullException("certificates");
            }
            int index = 0;
            try
            {
                while (index < certificates.Length)
                {
                    this.Remove(certificates[index]);
                    index++;
                }
            }
            catch
            {
                for (int i = 0; i < index; i++)
                {
                    this.Add(certificates[i]);
                }
                throw;
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

        public X509Certificate2 this[int index]
        {
            get
            {
                return (X509Certificate2) base.List[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base.List[index] = value;
            }
        }

        internal delegate int FindProcDelegate(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, object pvCallbackData);
    }
}

