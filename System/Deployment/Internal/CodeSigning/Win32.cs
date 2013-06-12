namespace System.Deployment.Internal.CodeSigning
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal static class Win32
    {
        internal const int AXL_LIFETIME_SIGNING = 0x10;
        internal const int AXL_REVOCATION_CHECK_END_CERT_ONLY = 2;
        internal const int AXL_REVOCATION_CHECK_ENTIRE_CHAIN = 4;
        internal const int AXL_REVOCATION_NO_CHECK = 1;
        internal const int AXL_TRUST_MICROSOFT_ROOT_ONLY = 0x20;
        internal const int AXL_URL_CACHE_ONLY_RETRIEVAL = 8;
        internal const int CERT_E_CHAINING = -2146762486;
        internal const int CERT_E_UNTRUSTEDROOT = -2146762487;
        internal const string KERNEL32 = "kernel32.dll";
        internal const string MSCORWKS = "clr.dll";
        internal const int NTE_BAD_KEY = -2146893821;
        internal const int S_OK = 0;
        internal const int TRUST_E_ACTION_UNKNOWN = -2146762750;
        internal const int TRUST_E_BAD_DIGEST = -2146869232;
        internal const int TRUST_E_BASIC_CONSTRAINTS = -2146869223;
        internal const int TRUST_E_CERT_SIGNATURE = -2146869244;
        internal const int TRUST_E_COUNTER_SIGNER = -2146869245;
        internal const int TRUST_E_EXPLICIT_DISTRUST = -2146762479;
        internal const int TRUST_E_FAIL = -2146762485;
        internal const int TRUST_E_FINANCIAL_CRITERIA = -2146869218;
        internal const int TRUST_E_NO_SIGNER_CERT = -2146869246;
        internal const int TRUST_E_NOSIGNATURE = -2146762496;
        internal const int TRUST_E_PROVIDER_UNKNOWN = -2146762751;
        internal const int TRUST_E_SUBJECT_FORM_UNKNOWN = -2146762749;
        internal const int TRUST_E_SUBJECT_NOT_TRUSTED = -2146762748;
        internal const int TRUST_E_SYSTEM_ERROR = -2146869247;
        internal const int TRUST_E_TIME_STAMP = -2146869243;

        [DllImport("clr.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int _AxlGetIssuerPublicKeyHash([In] IntPtr pCertContext, [In, Out] ref IntPtr ppwszPublicKeyHash);
        [DllImport("clr.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int _AxlPublicKeyBlobToPublicKeyToken([In] ref CRYPT_DATA_BLOB pCspPublicKeyBlob, [In, Out] ref IntPtr ppwszPublicKeyToken);
        [DllImport("clr.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int _AxlRSAKeyValueToPublicKeyToken([In] ref CRYPT_DATA_BLOB pModulusBlob, [In] ref CRYPT_DATA_BLOB pExponentBlob, [In, Out] ref IntPtr ppwszPublicKeyToken);
        [DllImport("clr.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int CertFreeAuthenticodeSignerInfo([In] ref AXL_SIGNER_INFO pSignerInfo);
        [DllImport("clr.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int CertFreeAuthenticodeTimestamperInfo([In] ref AXL_TIMESTAMPER_INFO pTimestamperInfo);
        [DllImport("clr.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int CertTimestampAuthenticodeLicense([In] ref CRYPT_DATA_BLOB pSignedLicenseBlob, [In] string pwszTimestampURI, [In, Out] ref CRYPT_DATA_BLOB pTimestampSignatureBlob);
        [DllImport("clr.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int CertVerifyAuthenticodeLicense([In] ref CRYPT_DATA_BLOB pLicenseBlob, [In] uint dwFlags, [In, Out] ref AXL_SIGNER_INFO pSignerInfo, [In, Out] ref AXL_TIMESTAMPER_INFO pTimestamperInfo);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr GetProcessHeap();
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool HeapFree([In] IntPtr hHeap, [In] uint dwFlags, [In] IntPtr lpMem);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct AXL_SIGNER_INFO
        {
            internal uint cbSize;
            internal uint dwError;
            internal uint algHash;
            internal IntPtr pwszHash;
            internal IntPtr pwszDescription;
            internal IntPtr pwszDescriptionUrl;
            internal IntPtr pChainContext;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct AXL_TIMESTAMPER_INFO
        {
            internal uint cbSize;
            internal uint dwError;
            internal uint algHash;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftTimestamp;
            internal IntPtr pChainContext;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_DATA_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;
        }
    }
}

