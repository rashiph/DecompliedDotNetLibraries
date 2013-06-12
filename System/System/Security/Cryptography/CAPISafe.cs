namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal abstract class CAPISafe : CAPINative
    {
        protected CAPISafe()
        {
        }

        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertControlStore([In] SafeCertStoreHandle hCertStore, [In] uint dwFlags, [In] uint dwCtrlType, [In] IntPtr pvCtrlPara);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeCertContextHandle CertCreateCertificateContext([In] uint dwCertEncodingType, [In] SafeLocalAllocHandle pbCertEncoded, [In] uint cbCertEncoded);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeCertChainHandle CertDuplicateCertificateChain([In] IntPtr pChainContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeCertChainHandle CertDuplicateCertificateChain([In] SafeCertChainHandle pChainContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeCertContextHandle CertDuplicateCertificateContext([In] IntPtr pCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeCertContextHandle CertDuplicateCertificateContext([In] SafeCertContextHandle pCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeCertStoreHandle CertDuplicateStore([In] IntPtr hCertStore);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr CertFindExtension([In, MarshalAs(UnmanagedType.LPStr)] string pszObjId, [In] uint cExtensions, [In] IntPtr rgExtensions);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        protected internal static extern bool CertFreeCertificateContext([In] IntPtr pCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertGetCertificateChain([In] IntPtr hChainEngine, [In] SafeCertContextHandle pCertContext, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME pTime, [In] SafeCertStoreHandle hAdditionalStore, [In] ref CAPIBase.CERT_CHAIN_PARA pChainPara, [In] uint dwFlags, [In] IntPtr pvReserved, [In, Out] ref SafeCertChainHandle ppChainContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertGetCertificateContextProperty([In] SafeCertContextHandle pCertContext, [In] uint dwPropId, [In, Out] SafeLocalAllocHandle pvData, [In, Out] ref uint pcbData);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertGetIntendedKeyUsage([In] uint dwCertEncodingType, [In] IntPtr pCertInfo, [In] IntPtr pbKeyUsage, [In, Out] uint cbKeyUsage);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern uint CertGetNameStringW([In] SafeCertContextHandle pCertContext, [In] uint dwType, [In] uint dwFlags, [In] IntPtr pvTypePara, [In, Out] SafeLocalAllocHandle pszNameString, [In] uint cchNameString);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern uint CertGetPublicKeyLength([In] uint dwCertEncodingType, [In] IntPtr pPublicKey);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertGetValidUsages([In] uint cCerts, [In] IntPtr rghCerts, [Out] IntPtr cNumOIDs, [In, Out] SafeLocalAllocHandle rghOIDs, [In, Out] IntPtr pcbOIDs);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern uint CertNameToStrW([In] uint dwCertEncodingType, [In] IntPtr pName, [In] uint dwStrType, [In, Out] SafeLocalAllocHandle psz, [In] uint csz);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertSerializeCertificateStoreElement([In] SafeCertContextHandle pCertContext, [In] uint dwFlags, [In, Out] SafeLocalAllocHandle pbElement, [In, Out] IntPtr pcbElement);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertStrToNameW([In] uint dwCertEncodingType, [In] string pszX500, [In] uint dwStrType, [In] IntPtr pvReserved, [In, Out] IntPtr pbEncoded, [In, Out] ref uint pcbEncoded, [In, Out] IntPtr ppszError);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertVerifyCertificateChainPolicy([In] IntPtr pszPolicyOID, [In] SafeCertChainHandle pChainContext, [In] ref CAPIBase.CERT_CHAIN_POLICY_PARA pPolicyPara, [In, Out] ref CAPIBase.CERT_CHAIN_POLICY_STATUS pPolicyStatus);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int CertVerifyTimeValidity([In, Out] ref System.Runtime.InteropServices.ComTypes.FILETIME pTimeToVerify, [In] IntPtr pCertInfo);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptAcquireCertificatePrivateKey([In] SafeCertContextHandle pCert, [In] uint dwFlags, [In] IntPtr pvReserved, [In, Out] ref SafeCryptProvHandle phCryptProv, [In, Out] ref uint pdwKeySpec, [In, Out] ref bool pfCallerFreeProv);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptDecodeObject([In] uint dwCertEncodingType, [In] IntPtr lpszStructType, [In] IntPtr pbEncoded, [In] uint cbEncoded, [In] uint dwFlags, [In, Out] SafeLocalAllocHandle pvStructInfo, [In, Out] IntPtr pcbStructInfo);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptDecodeObject([In] uint dwCertEncodingType, [In] IntPtr lpszStructType, [In] byte[] pbEncoded, [In] uint cbEncoded, [In] uint dwFlags, [In, Out] SafeLocalAllocHandle pvStructInfo, [In, Out] IntPtr pcbStructInfo);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptEncodeObject([In] uint dwCertEncodingType, [In] IntPtr lpszStructType, [In] IntPtr pvStructInfo, [In, Out] SafeLocalAllocHandle pbEncoded, [In, Out] IntPtr pcbEncoded);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptEncodeObject([In] uint dwCertEncodingType, [In, MarshalAs(UnmanagedType.LPStr)] string lpszStructType, [In] IntPtr pvStructInfo, [In, Out] SafeLocalAllocHandle pbEncoded, [In, Out] IntPtr pcbEncoded);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr CryptFindOIDInfo([In] uint dwKeyType, [In] IntPtr pvKey, [In] OidGroup dwGroupId);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr CryptFindOIDInfo([In] uint dwKeyType, [In] SafeLocalAllocHandle pvKey, [In] OidGroup dwGroupId);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptFormatObject([In] uint dwCertEncodingType, [In] uint dwFormatType, [In] uint dwFormatStrType, [In] IntPtr pFormatStruct, [In] IntPtr lpszStructType, [In] byte[] pbEncoded, [In] uint cbEncoded, [In, Out] SafeLocalAllocHandle pbFormat, [In, Out] IntPtr pcbFormat);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptFormatObject([In] uint dwCertEncodingType, [In] uint dwFormatType, [In] uint dwFormatStrType, [In] IntPtr pFormatStruct, [In, MarshalAs(UnmanagedType.LPStr)] string lpszStructType, [In] byte[] pbEncoded, [In] uint cbEncoded, [In, Out] SafeLocalAllocHandle pbFormat, [In, Out] IntPtr pcbFormat);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptGetProvParam([In] SafeCryptProvHandle hProv, [In] uint dwParam, [In] IntPtr pbData, [In] IntPtr pdwDataLen, [In] uint dwFlags);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptHashCertificate([In] IntPtr hCryptProv, [In] uint Algid, [In] uint dwFlags, [In] IntPtr pbEncoded, [In] uint cbEncoded, [Out] IntPtr pbComputedHash, [In, Out] IntPtr pcbComputedHash);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptHashPublicKeyInfo([In] IntPtr hCryptProv, [In] uint Algid, [In] uint dwFlags, [In] uint dwCertEncodingType, [In] IntPtr pInfo, [Out] IntPtr pbComputedHash, [In, Out] IntPtr pcbComputedHash);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptMsgGetParam([In] SafeCryptMsgHandle hCryptMsg, [In] uint dwParamType, [In] uint dwIndex, [In, Out] IntPtr pvData, [In, Out] IntPtr pcbData);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptMsgGetParam([In] SafeCryptMsgHandle hCryptMsg, [In] uint dwParamType, [In] uint dwIndex, [In, Out] SafeLocalAllocHandle pvData, [In, Out] IntPtr pcbData);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeCryptMsgHandle CryptMsgOpenToDecode([In] uint dwMsgEncodingType, [In] uint dwFlags, [In] uint dwMsgType, [In] IntPtr hCryptProv, [In] IntPtr pRecipientInfo, [In] IntPtr pStreamInfo);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptMsgUpdate([In] SafeCryptMsgHandle hCryptMsg, [In] byte[] pbData, [In] uint cbData, [In] bool fFinal);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptMsgUpdate([In] SafeCryptMsgHandle hCryptMsg, [In] IntPtr pbData, [In] uint cbData, [In] bool fFinal);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CryptMsgVerifyCountersignatureEncoded([In] IntPtr hCryptProv, [In] uint dwEncodingType, [In] IntPtr pbSignerInfo, [In] uint cbSignerInfo, [In] IntPtr pbSignerInfoCountersignature, [In] uint cbSignerInfoCountersignature, [In] IntPtr pciCountersigner);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern uint FormatMessage([In] uint dwFlags, [In] IntPtr lpSource, [In] uint dwMessageId, [In] uint dwLanguageId, [In, Out] StringBuilder lpBuffer, [In] uint nSize, [In] IntPtr Arguments);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool FreeLibrary([In] IntPtr hModule);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr GetProcAddress([In] IntPtr hModule, [In, MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        [DllImport("kernel32.dll", EntryPoint="LoadLibraryA", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr LoadLibrary([In, MarshalAs(UnmanagedType.LPStr)] string lpFileName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeLocalAllocHandle LocalAlloc([In] uint uFlags, [In] IntPtr sizetdwBytes);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalFree(IntPtr handle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true)]
        internal static extern int LsaNtStatusToWinError([In] int status);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void SetLastError(uint dwErrorCode);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void ZeroMemory(IntPtr handle, uint length);
    }
}

