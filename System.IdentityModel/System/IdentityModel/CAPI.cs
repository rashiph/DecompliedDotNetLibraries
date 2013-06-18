namespace System.IdentityModel
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class CAPI
    {
        internal const string BCRYPT = "bcrypt.dll";
        internal const uint CERT_CHAIN_POLICY_BASE = 1;
        internal const uint CERT_CHAIN_POLICY_IGNORE_PEER_TRUST_FLAG = 0x1000;
        internal const uint CERT_CHAIN_POLICY_NT_AUTH = 6;
        internal const uint CERT_CHAIN_REVOCATION_ACCUMULATIVE_TIMEOUT = 0x8000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY = 0x80000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN = 0x20000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x40000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_END_CERT = 0x10000000;
        internal const uint CERT_COMPARE_ANY = 0;
        internal const uint CERT_COMPARE_NAME_STR_A = 7;
        internal const uint CERT_COMPARE_NAME_STR_W = 8;
        internal const uint CERT_COMPARE_SHA1_HASH = 1;
        internal const uint CERT_COMPARE_SHIFT = 0x10;
        internal const uint CERT_FIND_ANY = 0;
        internal const uint CERT_FIND_HASH = 0x10000;
        internal const uint CERT_FIND_ISSUER_STR = 0x80004;
        internal const uint CERT_FIND_ISSUER_STR_A = 0x70004;
        internal const uint CERT_FIND_ISSUER_STR_W = 0x80004;
        internal const uint CERT_FIND_SHA1_HASH = 0x10000;
        internal const uint CERT_FIND_SUBJECT_STR = 0x80007;
        internal const uint CERT_FIND_SUBJECT_STR_A = 0x70007;
        internal const uint CERT_FIND_SUBJECT_STR_W = 0x80007;
        internal const uint CERT_INFO_ISSUER_FLAG = 4;
        internal const uint CERT_INFO_SUBJECT_FLAG = 7;
        internal const uint CERT_STORE_ADD_ALWAYS = 4;
        internal const uint CERT_STORE_CREATE_NEW_FLAG = 0x2000;
        internal const uint CERT_STORE_ENUM_ARCHIVED_FLAG = 0x200;
        internal const uint CERT_STORE_MAXIMUM_ALLOWED_FLAG = 0x1000;
        internal const uint CERT_STORE_OPEN_EXISTING_FLAG = 0x4000;
        internal const uint CERT_STORE_PROV_MEMORY = 2;
        internal const uint CERT_STORE_PROV_SYSTEM = 10;
        internal const uint CERT_STORE_READONLY_FLAG = 0x8000;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER = 0x10000;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE = 0x20000;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ID = 2;
        internal const uint CERT_SYSTEM_STORE_LOCATION_SHIFT = 0x10;
        internal const uint CERT_TRUST_IS_PEER_TRUSTED = 0x800;
        internal const string CRYPT32 = "crypt32.dll";
        internal const uint HCCE_CURRENT_USER = 0;
        internal const uint HCCE_LOCAL_MACHINE = 1;
        internal const uint PKCS_7_ASN_ENCODING = 0x10000;
        internal const int S_FALSE = 1;
        internal const int S_OK = 0;
        internal const string SubjectKeyIdentifierOid = "2.5.29.14";
        internal const uint USAGE_MATCH_TYPE_AND = 0;
        internal const uint USAGE_MATCH_TYPE_OR = 1;
        internal const uint X509_ASN_ENCODING = 1;

        [DllImport("bcrypt.dll", SetLastError=true)]
        internal static extern int BCryptGetFipsAlgorithmMode([MarshalAs(UnmanagedType.U1)] out bool pfEnabled);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertAddCertificateLinkToStore([In] SafeCertStoreHandle hCertStore, [In] IntPtr pCertContext, [In] uint dwAddDisposition, [In, Out] SafeCertContextHandle ppStoreContext);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true)]
        internal static extern bool CertCloseStore([In] IntPtr hCertStore, [In] uint dwFlags);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true)]
        internal static extern SafeCertContextHandle CertFindCertificateInStore([In] SafeCertStoreHandle hCertStore, [In] uint dwCertEncodingType, [In] uint dwFindFlags, [In] uint dwFindType, [In] SafeHGlobalHandle pvFindPara, [In] SafeCertContextHandle pPrevCertContext);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true)]
        internal static extern void CertFreeCertificateChain(IntPtr handle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true)]
        internal static extern bool CertFreeCertificateContext([In] IntPtr pCertContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertGetCertificateChain([In] IntPtr hChainEngine, [In] IntPtr pCertContext, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME pTime, [In] SafeCertStoreHandle hAdditionalStore, [In] ref CERT_CHAIN_PARA pChainPara, [In] uint dwFlags, [In] IntPtr pvReserved, out SafeCertChainHandle ppChainContext);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern SafeCertStoreHandle CertOpenStore([In] IntPtr lpszStoreProvider, [In] uint dwMsgAndCertEncodingType, [In] IntPtr hCryptProv, [In] uint dwFlags, [In] string pvPara);
        [DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CertVerifyCertificateChainPolicy([In] IntPtr pszPolicyOID, [In] SafeCertChainHandle pChainContext, [In] ref CERT_CHAIN_POLICY_PARA pPolicyPara, [In, Out] ref CERT_CHAIN_POLICY_STATUS pPolicyStatus);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_CONTEXT
        {
            internal uint cbSize;
            internal uint dwErrorStatus;
            internal uint dwInfoStatus;
            internal uint cChain;
            internal IntPtr rgpChain;
            internal uint cLowerQualityChainContext;
            internal IntPtr rgpLowerQualityChainContext;
            internal uint fHasRevocationFreshnessTime;
            internal uint dwRevocationFreshnessTime;
            internal CERT_CHAIN_CONTEXT(int size)
            {
                this.cbSize = (uint) size;
                this.dwErrorStatus = 0;
                this.dwInfoStatus = 0;
                this.cChain = 0;
                this.rgpChain = IntPtr.Zero;
                this.cLowerQualityChainContext = 0;
                this.rgpLowerQualityChainContext = IntPtr.Zero;
                this.fHasRevocationFreshnessTime = 0;
                this.dwRevocationFreshnessTime = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_PARA
        {
            internal uint cbSize;
            internal CAPI.CERT_USAGE_MATCH RequestedUsage;
            internal CAPI.CERT_USAGE_MATCH RequestedIssuancePolicy;
            internal uint dwUrlRetrievalTimeout;
            internal bool fCheckRevocationFreshnessTime;
            internal uint dwRevocationFreshnessTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_PARA
        {
            internal uint cbSize;
            internal uint dwFlags;
            internal IntPtr pvExtraPolicyPara;
            internal CERT_CHAIN_POLICY_PARA(int size)
            {
                this.cbSize = (uint) size;
                this.dwFlags = 0;
                this.pvExtraPolicyPara = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_STATUS
        {
            internal uint cbSize;
            internal uint dwError;
            internal IntPtr lChainIndex;
            internal IntPtr lElementIndex;
            internal IntPtr pvExtraPolicyStatus;
            internal CERT_CHAIN_POLICY_STATUS(int size)
            {
                this.cbSize = (uint) size;
                this.dwError = 0;
                this.lChainIndex = IntPtr.Zero;
                this.lElementIndex = IntPtr.Zero;
                this.pvExtraPolicyStatus = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CONTEXT
        {
            internal uint dwCertEncodingType;
            internal IntPtr pbCertEncoded;
            internal uint cbCertEncoded;
            internal IntPtr pCertInfo;
            internal IntPtr hCertStore;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ENHKEY_USAGE
        {
            internal uint cUsageIdentifier;
            internal IntPtr rgpszUsageIdentifier;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_USAGE_MATCH
        {
            internal uint dwType;
            internal CAPI.CERT_ENHKEY_USAGE Usage;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPTOAPI_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;
            internal static int Size;
            static CRYPTOAPI_BLOB()
            {
                Size = Marshal.SizeOf(typeof(CAPI.CRYPTOAPI_BLOB));
            }
        }
    }
}

