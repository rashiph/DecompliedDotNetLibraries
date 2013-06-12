namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal abstract class CAPIUnsafe : CAPISafe
    {
        protected CAPIUnsafe()
        {
        }

        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertAddCertificateContextToStore([In] SafeCertStoreHandle hCertStore, [In] SafeCertContextHandle pCertContext, [In] uint dwAddDisposition, [In, Out] SafeCertContextHandle ppStoreContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertAddCertificateLinkToStore([In] SafeCertStoreHandle hCertStore, [In] SafeCertContextHandle pCertContext, [In] uint dwAddDisposition, [In, Out] SafeCertContextHandle ppStoreContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern SafeCertContextHandle CertCreateSelfSignCertificate([In] SafeCryptProvHandle hProv, [In] IntPtr pSubjectIssuerBlob, [In] uint dwFlags, [In] IntPtr pKeyProvInfo, [In] IntPtr pSignatureAlgorithm, [In] IntPtr pStartTime, [In] IntPtr pEndTime, [In] IntPtr pExtensions);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertDeleteCertificateFromStore([In] SafeCertContextHandle pCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern IntPtr CertEnumCertificatesInStore([In] SafeCertStoreHandle hCertStore, [In] IntPtr pPrevCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern SafeCertContextHandle CertEnumCertificatesInStore([In] SafeCertStoreHandle hCertStore, [In] SafeCertContextHandle pPrevCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern SafeCertContextHandle CertFindCertificateInStore([In] SafeCertStoreHandle hCertStore, [In] uint dwCertEncodingType, [In] uint dwFindFlags, [In] uint dwFindType, [In] IntPtr pvFindPara, [In] SafeCertContextHandle pPrevCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        protected internal static extern SafeCertStoreHandle CertOpenStore([In] IntPtr lpszStoreProvider, [In] uint dwMsgAndCertEncodingType, [In] IntPtr hCryptProv, [In] uint dwFlags, [In] string pvPara);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertSaveStore([In] SafeCertStoreHandle hCertStore, [In] uint dwMsgAndCertEncodingType, [In] uint dwSaveAs, [In] uint dwSaveTo, [In, Out] IntPtr pvSaveToPara, [In] uint dwFlags);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertSetCertificateContextProperty([In] IntPtr pCertContext, [In] uint dwPropId, [In] uint dwFlags, [In] IntPtr pvData);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertSetCertificateContextProperty([In] SafeCertContextHandle pCertContext, [In] uint dwPropId, [In] uint dwFlags, [In] IntPtr pvData);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertSetCertificateContextProperty([In] SafeCertContextHandle pCertContext, [In] uint dwPropId, [In] uint dwFlags, [In] SafeLocalAllocHandle safeLocalAllocHandle);
        [DllImport("advapi32.dll", EntryPoint="CryptAcquireContextA", CharSet=CharSet.Auto)]
        protected internal static extern bool CryptAcquireContext([In, Out] ref SafeCryptProvHandle hCryptProv, [In, MarshalAs(UnmanagedType.LPStr)] string pszContainer, [In, MarshalAs(UnmanagedType.LPStr)] string pszProvider, [In] uint dwProvType, [In] uint dwFlags);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CryptMsgControl([In] SafeCryptMsgHandle hCryptMsg, [In] uint dwFlags, [In] uint dwCtrlType, [In] IntPtr pvCtrlPara);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CryptMsgCountersign([In] SafeCryptMsgHandle hCryptMsg, [In] uint dwIndex, [In] uint cCountersigners, [In] IntPtr rgCountersigners);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern SafeCryptMsgHandle CryptMsgOpenToEncode([In] uint dwMsgEncodingType, [In] uint dwFlags, [In] uint dwMsgType, [In] IntPtr pvMsgEncodeInfo, [In] IntPtr pszInnerContentObjID, [In] IntPtr pStreamInfo);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern SafeCryptMsgHandle CryptMsgOpenToEncode([In] uint dwMsgEncodingType, [In] uint dwFlags, [In] uint dwMsgType, [In] IntPtr pvMsgEncodeInfo, [In, MarshalAs(UnmanagedType.LPStr)] string pszInnerContentObjID, [In] IntPtr pStreamInfo);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptProtectData([In] IntPtr pDataIn, [In] string szDataDescr, [In] IntPtr pOptionalEntropy, [In] IntPtr pvReserved, [In] IntPtr pPromptStruct, [In] uint dwFlags, [In, Out] IntPtr pDataBlob);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CryptQueryObject([In] uint dwObjectType, [In] IntPtr pvObject, [In] uint dwExpectedContentTypeFlags, [In] uint dwExpectedFormatTypeFlags, [In] uint dwFlags, [Out] IntPtr pdwMsgAndCertEncodingType, [Out] IntPtr pdwContentType, [Out] IntPtr pdwFormatType, [In, Out] IntPtr phCertStore, [In, Out] IntPtr phMsg, [In, Out] IntPtr ppvContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CryptQueryObject([In] uint dwObjectType, [In] IntPtr pvObject, [In] uint dwExpectedContentTypeFlags, [In] uint dwExpectedFormatTypeFlags, [In] uint dwFlags, [Out] IntPtr pdwMsgAndCertEncodingType, [Out] IntPtr pdwContentType, [Out] IntPtr pdwFormatType, [In, Out] ref SafeCertStoreHandle phCertStore, [In, Out] IntPtr phMsg, [In, Out] IntPtr ppvContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptUnprotectData([In] IntPtr pDataIn, [In] IntPtr ppszDataDescr, [In] IntPtr pOptionalEntropy, [In] IntPtr pvReserved, [In] IntPtr pPromptStruct, [In] uint dwFlags, [In, Out] IntPtr pDataBlob);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        protected internal static extern bool PFXExportCertStore([In] SafeCertStoreHandle hStore, [In, Out] IntPtr pPFX, [In] string szPassword, [In] uint dwFlags);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        protected internal static extern SafeCertStoreHandle PFXImportCertStore([In] IntPtr pPFX, [In] string szPassword, [In] uint dwFlags);
    }
}

