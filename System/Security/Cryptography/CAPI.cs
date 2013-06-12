namespace System.Security.Cryptography
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    internal sealed class CAPI : CAPIMethods
    {
        internal static byte[] BlobToByteArray(IntPtr pBlob)
        {
            CAPIBase.CRYPTOAPI_BLOB blob = (CAPIBase.CRYPTOAPI_BLOB) Marshal.PtrToStructure(pBlob, typeof(CAPIBase.CRYPTOAPI_BLOB));
            if (blob.cbData == 0)
            {
                return new byte[0];
            }
            return BlobToByteArray(blob);
        }

        internal static byte[] BlobToByteArray(CAPIBase.CRYPTOAPI_BLOB blob)
        {
            if (blob.cbData == 0)
            {
                return new byte[0];
            }
            byte[] destination = new byte[blob.cbData];
            Marshal.Copy(blob.pbData, destination, 0, destination.Length);
            return destination;
        }

        internal static bool CertAddCertificateContextToStore([In] System.Security.Cryptography.SafeCertStoreHandle hCertStore, [In] System.Security.Cryptography.SafeCertContextHandle pCertContext, [In] uint dwAddDisposition, [In, Out] System.Security.Cryptography.SafeCertContextHandle ppStoreContext)
        {
            if (hCertStore == null)
            {
                throw new ArgumentNullException("hCertStore");
            }
            if (hCertStore.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "hCertStore");
            }
            if (pCertContext == null)
            {
                throw new ArgumentNullException("pCertContext");
            }
            if (pCertContext.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "pCertContext");
            }
            new StorePermission(StorePermissionFlags.AddToStore).Demand();
            return CAPIUnsafe.CertAddCertificateContextToStore(hCertStore, pCertContext, dwAddDisposition, ppStoreContext);
        }

        internal static bool CertAddCertificateLinkToStore([In] System.Security.Cryptography.SafeCertStoreHandle hCertStore, [In] System.Security.Cryptography.SafeCertContextHandle pCertContext, [In] uint dwAddDisposition, [In, Out] System.Security.Cryptography.SafeCertContextHandle ppStoreContext)
        {
            if (hCertStore == null)
            {
                throw new ArgumentNullException("hCertStore");
            }
            if (hCertStore.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "hCertStore");
            }
            if (pCertContext == null)
            {
                throw new ArgumentNullException("pCertContext");
            }
            if (pCertContext.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "pCertContext");
            }
            new StorePermission(StorePermissionFlags.AddToStore).Demand();
            return CAPIUnsafe.CertAddCertificateLinkToStore(hCertStore, pCertContext, dwAddDisposition, ppStoreContext);
        }

        internal static bool CertDeleteCertificateFromStore([In] System.Security.Cryptography.SafeCertContextHandle pCertContext)
        {
            if (pCertContext == null)
            {
                throw new ArgumentNullException("pCertContext");
            }
            if (pCertContext.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "pCertContext");
            }
            new StorePermission(StorePermissionFlags.RemoveFromStore).Demand();
            return CAPIUnsafe.CertDeleteCertificateFromStore(pCertContext);
        }

        internal static System.Security.Cryptography.SafeCertContextHandle CertDuplicateCertificateContext([In] IntPtr pCertContext)
        {
            if (pCertContext == IntPtr.Zero)
            {
                return System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            }
            return CAPISafe.CertDuplicateCertificateContext(pCertContext);
        }

        internal static System.Security.Cryptography.SafeCertContextHandle CertDuplicateCertificateContext([In] System.Security.Cryptography.SafeCertContextHandle pCertContext)
        {
            if ((pCertContext != null) && !pCertContext.IsInvalid)
            {
                return CAPISafe.CertDuplicateCertificateContext(pCertContext);
            }
            return System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
        }

        internal static IntPtr CertEnumCertificatesInStore([In] System.Security.Cryptography.SafeCertStoreHandle hCertStore, [In] IntPtr pPrevCertContext)
        {
            if (hCertStore == null)
            {
                throw new ArgumentNullException("hCertStore");
            }
            if (hCertStore.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "hCertStore");
            }
            if (pPrevCertContext == IntPtr.Zero)
            {
                new StorePermission(StorePermissionFlags.EnumerateCertificates).Demand();
            }
            IntPtr pCertContext = CAPIUnsafe.CertEnumCertificatesInStore(hCertStore, pPrevCertContext);
            if (pCertContext == IntPtr.Zero)
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr != -2146885628)
                {
                    CAPISafe.CertFreeCertificateContext(pCertContext);
                    throw new CryptographicException(hr);
                }
            }
            return pCertContext;
        }

        internal static System.Security.Cryptography.SafeCertContextHandle CertEnumCertificatesInStore([In] System.Security.Cryptography.SafeCertStoreHandle hCertStore, [In] System.Security.Cryptography.SafeCertContextHandle pPrevCertContext)
        {
            if (hCertStore == null)
            {
                throw new ArgumentNullException("hCertStore");
            }
            if (hCertStore.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "hCertStore");
            }
            if (pPrevCertContext.IsInvalid)
            {
                new StorePermission(StorePermissionFlags.EnumerateCertificates).Demand();
            }
            System.Security.Cryptography.SafeCertContextHandle handle = CAPIUnsafe.CertEnumCertificatesInStore(hCertStore, pPrevCertContext);
            if (((handle == null) || handle.IsInvalid) && (Marshal.GetLastWin32Error() != -2146885628))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return handle;
        }

        internal static System.Security.Cryptography.SafeCertContextHandle CertFindCertificateInStore([In] System.Security.Cryptography.SafeCertStoreHandle hCertStore, [In] uint dwCertEncodingType, [In] uint dwFindFlags, [In] uint dwFindType, [In] IntPtr pvFindPara, [In] System.Security.Cryptography.SafeCertContextHandle pPrevCertContext)
        {
            if (hCertStore == null)
            {
                throw new ArgumentNullException("hCertStore");
            }
            if (hCertStore.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "hCertStore");
            }
            return CAPIUnsafe.CertFindCertificateInStore(hCertStore, dwCertEncodingType, dwFindFlags, dwFindType, pvFindPara, pPrevCertContext);
        }

        internal static System.Security.Cryptography.SafeCertStoreHandle CertOpenStore([In] IntPtr lpszStoreProvider, [In] uint dwMsgAndCertEncodingType, [In] IntPtr hCryptProv, [In] uint dwFlags, [In] string pvPara)
        {
            if ((lpszStoreProvider != new IntPtr(2L)) && (lpszStoreProvider != new IntPtr(10L)))
            {
                throw new ArgumentException(SR.GetString("Security_InvalidValue"), "lpszStoreProvider");
            }
            if (((((dwFlags & 0x20000) == 0x20000) || ((dwFlags & 0x80000) == 0x80000)) || ((dwFlags & 0x90000) == 0x90000)) && ((pvPara != null) && pvPara.StartsWith(@"\\", StringComparison.Ordinal)))
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
            }
            if ((dwFlags & 0x10) == 0x10)
            {
                new StorePermission(StorePermissionFlags.DeleteStore).Demand();
            }
            else
            {
                new StorePermission(StorePermissionFlags.OpenStore).Demand();
            }
            if ((dwFlags & 0x2000) == 0x2000)
            {
                new StorePermission(StorePermissionFlags.CreateStore).Demand();
            }
            if ((dwFlags & 0x4000) == 0)
            {
                new StorePermission(StorePermissionFlags.CreateStore).Demand();
            }
            return CAPIUnsafe.CertOpenStore(lpszStoreProvider, dwMsgAndCertEncodingType, hCryptProv, dwFlags | 4, pvPara);
        }

        internal static bool CertSaveStore([In] System.Security.Cryptography.SafeCertStoreHandle hCertStore, [In] uint dwMsgAndCertEncodingType, [In] uint dwSaveAs, [In] uint dwSaveTo, [In, Out] IntPtr pvSaveToPara, [In] uint dwFlags)
        {
            if (hCertStore == null)
            {
                throw new ArgumentNullException("hCertStore");
            }
            if (hCertStore.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "hCertStore");
            }
            new StorePermission(StorePermissionFlags.EnumerateCertificates).Demand();
            if ((dwSaveTo == 3) || (dwSaveTo == 4))
            {
                throw new ArgumentException(SR.GetString("Security_InvalidValue"), "pvSaveToPara");
            }
            return CAPIUnsafe.CertSaveStore(hCertStore, dwMsgAndCertEncodingType, dwSaveAs, dwSaveTo, pvSaveToPara, dwFlags);
        }

        internal static bool CertSetCertificateContextProperty([In] IntPtr pCertContext, [In] uint dwPropId, [In] uint dwFlags, [In] IntPtr pvData)
        {
            if (pvData == IntPtr.Zero)
            {
                throw new ArgumentNullException("pvData");
            }
            if (((dwPropId != 0x13) && (dwPropId != 11)) && ((dwPropId != 0x65) && (dwPropId != 2)))
            {
                throw new ArgumentException(SR.GetString("Security_InvalidValue"), "dwFlags");
            }
            if (((dwPropId == 0x13) || (dwPropId == 11)) || (dwPropId == 2))
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
            }
            return CAPIUnsafe.CertSetCertificateContextProperty(pCertContext, dwPropId, dwFlags, pvData);
        }

        internal static bool CertSetCertificateContextProperty([In] System.Security.Cryptography.SafeCertContextHandle pCertContext, [In] uint dwPropId, [In] uint dwFlags, [In] IntPtr pvData)
        {
            if (pvData == IntPtr.Zero)
            {
                throw new ArgumentNullException("pvData");
            }
            if (((dwPropId != 0x13) && (dwPropId != 11)) && ((dwPropId != 0x65) && (dwPropId != 2)))
            {
                throw new ArgumentException(SR.GetString("Security_InvalidValue"), "dwFlags");
            }
            if (((dwPropId == 0x13) || (dwPropId == 11)) || (dwPropId == 2))
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
            }
            return CAPIUnsafe.CertSetCertificateContextProperty(pCertContext, dwPropId, dwFlags, pvData);
        }

        internal static bool CertSetCertificateContextProperty([In] System.Security.Cryptography.SafeCertContextHandle pCertContext, [In] uint dwPropId, [In] uint dwFlags, [In] SafeLocalAllocHandle safeLocalAllocHandle)
        {
            if (pCertContext == null)
            {
                throw new ArgumentNullException("pCertContext");
            }
            if (pCertContext.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "pCertContext");
            }
            if (((dwPropId != 0x13) && (dwPropId != 11)) && ((dwPropId != 0x65) && (dwPropId != 2)))
            {
                throw new ArgumentException(SR.GetString("Security_InvalidValue"), "dwFlags");
            }
            if (((dwPropId == 0x13) || (dwPropId == 11)) || (dwPropId == 2))
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
            }
            return CAPIUnsafe.CertSetCertificateContextProperty(pCertContext, dwPropId, dwFlags, safeLocalAllocHandle);
        }

        internal static bool CryptAcquireContext(ref SafeCryptProvHandle hCryptProv, IntPtr pwszContainer, IntPtr pwszProvider, uint dwProvType, uint dwFlags)
        {
            string str = null;
            if (pwszContainer != IntPtr.Zero)
            {
                str = Marshal.PtrToStringUni(pwszContainer);
            }
            string str2 = null;
            if (pwszProvider != IntPtr.Zero)
            {
                str2 = Marshal.PtrToStringUni(pwszProvider);
            }
            return CryptAcquireContext(ref hCryptProv, str, str2, dwProvType, dwFlags);
        }

        internal static bool CryptAcquireContext([In, Out] ref SafeCryptProvHandle hCryptProv, [In, MarshalAs(UnmanagedType.LPStr)] string pwszContainer, [In, MarshalAs(UnmanagedType.LPStr)] string pwszProvider, [In] uint dwProvType, [In] uint dwFlags)
        {
            CspParameters parameters = new CspParameters {
                ProviderName = pwszProvider,
                KeyContainerName = pwszContainer,
                ProviderType = (int) dwProvType,
                KeyNumber = -1,
                Flags = ((dwFlags & 0x20) == 0x20) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags
            };
            KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
            KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Open);
            permission.AccessEntries.Add(accessEntry);
            permission.Demand();
            bool flag = CAPIUnsafe.CryptAcquireContext(ref hCryptProv, pwszContainer, pwszProvider, dwProvType, dwFlags);
            if (!flag && (Marshal.GetLastWin32Error() == -2146893802))
            {
                flag = CAPIUnsafe.CryptAcquireContext(ref hCryptProv, pwszContainer, pwszProvider, dwProvType, dwFlags | 8);
            }
            return flag;
        }

        internal static CAPIBase.CRYPT_OID_INFO CryptFindOIDInfo([In] uint dwKeyType, [In] IntPtr pvKey, [In] System.Security.Cryptography.OidGroup dwGroupId)
        {
            if (pvKey == IntPtr.Zero)
            {
                throw new ArgumentNullException("pvKey");
            }
            CAPIBase.CRYPT_OID_INFO crypt_oid_info = new CAPIBase.CRYPT_OID_INFO(Marshal.SizeOf(typeof(CAPIBase.CRYPT_OID_INFO)));
            IntPtr ptr = CAPISafe.CryptFindOIDInfo(dwKeyType, pvKey, dwGroupId);
            if (ptr != IntPtr.Zero)
            {
                crypt_oid_info = (CAPIBase.CRYPT_OID_INFO) Marshal.PtrToStructure(ptr, typeof(CAPIBase.CRYPT_OID_INFO));
            }
            return crypt_oid_info;
        }

        internal static CAPIBase.CRYPT_OID_INFO CryptFindOIDInfo([In] uint dwKeyType, [In] SafeLocalAllocHandle pvKey, [In] System.Security.Cryptography.OidGroup dwGroupId)
        {
            if (pvKey == null)
            {
                throw new ArgumentNullException("pvKey");
            }
            if (pvKey.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "pvKey");
            }
            CAPIBase.CRYPT_OID_INFO crypt_oid_info = new CAPIBase.CRYPT_OID_INFO(Marshal.SizeOf(typeof(CAPIBase.CRYPT_OID_INFO)));
            IntPtr ptr = CAPISafe.CryptFindOIDInfo(dwKeyType, pvKey, dwGroupId);
            if (ptr != IntPtr.Zero)
            {
                crypt_oid_info = (CAPIBase.CRYPT_OID_INFO) Marshal.PtrToStructure(ptr, typeof(CAPIBase.CRYPT_OID_INFO));
            }
            return crypt_oid_info;
        }

        internal static unsafe string CryptFormatObject([In] uint dwCertEncodingType, [In] uint dwFormatStrType, [In] IntPtr lpszStructType, [In] byte[] rawData)
        {
            if (rawData == null)
            {
                throw new ArgumentNullException("rawData");
            }
            uint num = 0;
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (!CAPISafe.CryptFormatObject(dwCertEncodingType, 0, dwFormatStrType, IntPtr.Zero, lpszStructType, rawData, (uint) rawData.Length, invalidHandle, new IntPtr((void*) &num)))
            {
                return System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(rawData);
            }
            invalidHandle = LocalAlloc(0, new IntPtr((long) num));
            if (!CAPISafe.CryptFormatObject(dwCertEncodingType, 0, dwFormatStrType, IntPtr.Zero, lpszStructType, rawData, (uint) rawData.Length, invalidHandle, new IntPtr((void*) &num)))
            {
                return System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(rawData);
            }
            string str = Marshal.PtrToStringUni(invalidHandle.DangerousGetHandle());
            invalidHandle.Dispose();
            return str;
        }

        internal static unsafe string CryptFormatObject([In] uint dwCertEncodingType, [In] uint dwFormatStrType, [In] string lpszStructType, [In] byte[] rawData)
        {
            if (rawData == null)
            {
                throw new ArgumentNullException("rawData");
            }
            uint num = 0;
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (!CAPISafe.CryptFormatObject(dwCertEncodingType, 0, dwFormatStrType, IntPtr.Zero, lpszStructType, rawData, (uint) rawData.Length, invalidHandle, new IntPtr((void*) &num)))
            {
                return System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(rawData);
            }
            invalidHandle = LocalAlloc(0, new IntPtr((long) num));
            if (!CAPISafe.CryptFormatObject(dwCertEncodingType, 0, dwFormatStrType, IntPtr.Zero, lpszStructType, rawData, (uint) rawData.Length, invalidHandle, new IntPtr((void*) &num)))
            {
                return System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(rawData);
            }
            string str = Marshal.PtrToStringUni(invalidHandle.DangerousGetHandle());
            invalidHandle.Dispose();
            return str;
        }

        internal static bool CryptMsgControl([In] SafeCryptMsgHandle hCryptMsg, [In] uint dwFlags, [In] uint dwCtrlType, [In] IntPtr pvCtrlPara)
        {
            return CAPIUnsafe.CryptMsgControl(hCryptMsg, dwFlags, dwCtrlType, pvCtrlPara);
        }

        internal static bool CryptMsgCountersign([In] SafeCryptMsgHandle hCryptMsg, [In] uint dwIndex, [In] uint cCountersigners, [In] IntPtr rgCountersigners)
        {
            return CAPIUnsafe.CryptMsgCountersign(hCryptMsg, dwIndex, cCountersigners, rgCountersigners);
        }

        internal static SafeCryptMsgHandle CryptMsgOpenToEncode([In] uint dwMsgEncodingType, [In] uint dwFlags, [In] uint dwMsgType, [In] IntPtr pvMsgEncodeInfo, [In] IntPtr pszInnerContentObjID, [In] IntPtr pStreamInfo)
        {
            return CAPIUnsafe.CryptMsgOpenToEncode(dwMsgEncodingType, dwFlags, dwMsgType, pvMsgEncodeInfo, pszInnerContentObjID, pStreamInfo);
        }

        internal static SafeCryptMsgHandle CryptMsgOpenToEncode([In] uint dwMsgEncodingType, [In] uint dwFlags, [In] uint dwMsgType, [In] IntPtr pvMsgEncodeInfo, [In] string pszInnerContentObjID, [In] IntPtr pStreamInfo)
        {
            return CAPIUnsafe.CryptMsgOpenToEncode(dwMsgEncodingType, dwFlags, dwMsgType, pvMsgEncodeInfo, pszInnerContentObjID, pStreamInfo);
        }

        internal static unsafe bool CryptQueryObject([In] uint dwObjectType, [In] object pvObject, [In] uint dwExpectedContentTypeFlags, [In] uint dwExpectedFormatTypeFlags, [In] uint dwFlags, [Out] IntPtr pdwMsgAndCertEncodingType, [Out] IntPtr pdwContentType, [Out] IntPtr pdwFormatType, [In, Out] IntPtr phCertStore, [In, Out] IntPtr phMsg, [In, Out] IntPtr ppvContext)
        {
            bool flag = false;
            GCHandle handle = GCHandle.Alloc(pvObject, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            try
            {
                if (pvObject == null)
                {
                    throw new ArgumentNullException("pvObject");
                }
                if (dwObjectType == 1)
                {
                    string fullPath = Path.GetFullPath((string) pvObject);
                    new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
                }
                else
                {
                    CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob;
                    cryptoapi_blob.cbData = (uint) ((byte[]) pvObject).Length;
                    cryptoapi_blob.pbData = ptr;
                    ptr = new IntPtr((void*) &cryptoapi_blob);
                }
                flag = CAPIUnsafe.CryptQueryObject(dwObjectType, ptr, dwExpectedContentTypeFlags, dwExpectedFormatTypeFlags, dwFlags, pdwMsgAndCertEncodingType, pdwContentType, pdwFormatType, phCertStore, phMsg, ppvContext);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            return flag;
        }

        internal static unsafe bool CryptQueryObject([In] uint dwObjectType, [In] object pvObject, [In] uint dwExpectedContentTypeFlags, [In] uint dwExpectedFormatTypeFlags, [In] uint dwFlags, [Out] IntPtr pdwMsgAndCertEncodingType, [Out] IntPtr pdwContentType, [Out] IntPtr pdwFormatType, [In, Out] ref System.Security.Cryptography.SafeCertStoreHandle phCertStore, [In, Out] IntPtr phMsg, [In, Out] IntPtr ppvContext)
        {
            bool flag = false;
            GCHandle handle = GCHandle.Alloc(pvObject, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            try
            {
                if (pvObject == null)
                {
                    throw new ArgumentNullException("pvObject");
                }
                if (dwObjectType == 1)
                {
                    string fullPath = Path.GetFullPath((string) pvObject);
                    new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
                }
                else
                {
                    CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob;
                    cryptoapi_blob.cbData = (uint) ((byte[]) pvObject).Length;
                    cryptoapi_blob.pbData = ptr;
                    ptr = new IntPtr((void*) &cryptoapi_blob);
                }
                flag = CAPIUnsafe.CryptQueryObject(dwObjectType, ptr, dwExpectedContentTypeFlags, dwExpectedFormatTypeFlags, dwFlags, pdwMsgAndCertEncodingType, pdwContentType, pdwFormatType, ref phCertStore, phMsg, ppvContext);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            return flag;
        }

        internal static unsafe bool DecodeObject(IntPtr pszStructType, byte[] pbEncoded, out SafeLocalAllocHandle decodedValue, out uint cbDecodedValue)
        {
            decodedValue = SafeLocalAllocHandle.InvalidHandle;
            cbDecodedValue = 0;
            uint num = 0;
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (!CAPISafe.CryptDecodeObject(0x10001, pszStructType, pbEncoded, (uint) pbEncoded.Length, 0, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            invalidHandle = LocalAlloc(0, new IntPtr((long) num));
            if (!CAPISafe.CryptDecodeObject(0x10001, pszStructType, pbEncoded, (uint) pbEncoded.Length, 0, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            decodedValue = invalidHandle;
            cbDecodedValue = num;
            return true;
        }

        internal static unsafe bool DecodeObject(IntPtr pszStructType, IntPtr pbEncoded, uint cbEncoded, out SafeLocalAllocHandle decodedValue, out uint cbDecodedValue)
        {
            decodedValue = SafeLocalAllocHandle.InvalidHandle;
            cbDecodedValue = 0;
            uint num = 0;
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (!CAPISafe.CryptDecodeObject(0x10001, pszStructType, pbEncoded, cbEncoded, 0, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            invalidHandle = LocalAlloc(0, new IntPtr((long) num));
            if (!CAPISafe.CryptDecodeObject(0x10001, pszStructType, pbEncoded, cbEncoded, 0, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            decodedValue = invalidHandle;
            cbDecodedValue = num;
            return true;
        }

        internal static unsafe bool EncodeObject(IntPtr lpszStructType, IntPtr pvStructInfo, out byte[] encodedData)
        {
            encodedData = new byte[0];
            uint num = 0;
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (!CAPISafe.CryptEncodeObject(0x10001, lpszStructType, pvStructInfo, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            invalidHandle = LocalAlloc(0, new IntPtr((long) num));
            if (!CAPISafe.CryptEncodeObject(0x10001, lpszStructType, pvStructInfo, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            encodedData = new byte[num];
            Marshal.Copy(invalidHandle.DangerousGetHandle(), encodedData, 0, (int) num);
            invalidHandle.Dispose();
            return true;
        }

        internal static unsafe bool EncodeObject(string lpszStructType, IntPtr pvStructInfo, out byte[] encodedData)
        {
            encodedData = new byte[0];
            uint num = 0;
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (!CAPISafe.CryptEncodeObject(0x10001, lpszStructType, pvStructInfo, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            invalidHandle = LocalAlloc(0, new IntPtr((long) num));
            if (!CAPISafe.CryptEncodeObject(0x10001, lpszStructType, pvStructInfo, invalidHandle, new IntPtr((void*) &num)))
            {
                return false;
            }
            encodedData = new byte[num];
            Marshal.Copy(invalidHandle.DangerousGetHandle(), encodedData, 0, (int) num);
            invalidHandle.Dispose();
            return true;
        }

        internal static unsafe string GetCertNameInfo([In] System.Security.Cryptography.SafeCertContextHandle safeCertContext, [In] uint dwFlags, [In] uint dwDisplayType)
        {
            if (safeCertContext == null)
            {
                throw new ArgumentNullException("pCertContext");
            }
            if (safeCertContext.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "safeCertContext");
            }
            uint num = 0x2000003;
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            if (dwDisplayType == 3)
            {
                invalidHandle = System.Security.Cryptography.X509Certificates.X509Utils.StringToAnsiPtr("2.5.4.3");
            }
            uint cchNameString = 0;
            SafeLocalAllocHandle pszNameString = SafeLocalAllocHandle.InvalidHandle;
            cchNameString = CAPISafe.CertGetNameStringW(safeCertContext, dwDisplayType, dwFlags, (dwDisplayType == 3) ? invalidHandle.DangerousGetHandle() : new IntPtr((void*) &num), pszNameString, 0);
            if (cchNameString == 0)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            pszNameString = LocalAlloc(0, new IntPtr((long) (2 * cchNameString)));
            if (CAPISafe.CertGetNameStringW(safeCertContext, dwDisplayType, dwFlags, (dwDisplayType == 3) ? invalidHandle.DangerousGetHandle() : new IntPtr((void*) &num), pszNameString, cchNameString) == 0)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            string str = Marshal.PtrToStringUni(pszNameString.DangerousGetHandle());
            pszNameString.Dispose();
            invalidHandle.Dispose();
            return str;
        }

        internal static SafeLocalAllocHandle LocalAlloc(uint uFlags, IntPtr sizetdwBytes)
        {
            SafeLocalAllocHandle handle = CAPISafe.LocalAlloc(uFlags, sizetdwBytes);
            if ((handle == null) || handle.IsInvalid)
            {
                throw new OutOfMemoryException();
            }
            return handle;
        }

        internal static bool PFXExportCertStore([In] System.Security.Cryptography.SafeCertStoreHandle hCertStore, [In, Out] IntPtr pPFX, [In] string szPassword, [In] uint dwFlags)
        {
            if (hCertStore == null)
            {
                throw new ArgumentNullException("hCertStore");
            }
            if (hCertStore.IsInvalid)
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "hCertStore");
            }
            new KeyContainerPermission(KeyContainerPermissionFlags.Export | KeyContainerPermissionFlags.Open).Demand();
            return CAPIUnsafe.PFXExportCertStore(hCertStore, pPFX, szPassword, dwFlags);
        }

        internal static unsafe System.Security.Cryptography.SafeCertStoreHandle PFXImportCertStore([In] uint dwObjectType, [In] object pvObject, [In] string szPassword, [In] uint dwFlags, [In] bool persistKeyContainers)
        {
            if (pvObject == null)
            {
                throw new ArgumentNullException("pvObject");
            }
            byte[] buffer = null;
            if (dwObjectType == 1)
            {
                buffer = File.ReadAllBytes((string) pvObject);
            }
            else
            {
                buffer = (byte[]) pvObject;
            }
            if (persistKeyContainers)
            {
                new KeyContainerPermission(KeyContainerPermissionFlags.Create).Demand();
            }
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            GCHandle handle2 = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle2.AddrOfPinnedObject();
            try
            {
                CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob;
                cryptoapi_blob.cbData = (uint) buffer.Length;
                cryptoapi_blob.pbData = ptr;
                invalidHandle = CAPIUnsafe.PFXImportCertStore(new IntPtr((void*) &cryptoapi_blob), szPassword, dwFlags);
            }
            finally
            {
                if (handle2.IsAllocated)
                {
                    handle2.Free();
                }
            }
            if (!invalidHandle.IsInvalid && !persistKeyContainers)
            {
                for (IntPtr ptr2 = CertEnumCertificatesInStore(invalidHandle, IntPtr.Zero); ptr2 != IntPtr.Zero; ptr2 = CertEnumCertificatesInStore(invalidHandle, ptr2))
                {
                    CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob2 = new CAPIBase.CRYPTOAPI_BLOB();
                    if (!CertSetCertificateContextProperty(ptr2, 0x65, 0x40000000, new IntPtr((void*) &cryptoapi_blob2)))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                }
            }
            return invalidHandle;
        }
    }
}

