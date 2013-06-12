namespace System.Security.Cryptography.X509Certificates
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    internal static class X509Native
    {
        [StructLayout(LayoutKind.Sequential), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public struct AXL_AUTHENTICODE_SIGNER_INFO
        {
            public int cbSize;
            public int dwError;
            public CapiNative.AlgorithmId algHash;
            public IntPtr pwszHash;
            public IntPtr pwszDescription;
            public IntPtr pwszDescriptionUrl;
            public IntPtr pChainContext;
        }

        [StructLayout(LayoutKind.Sequential), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public struct AXL_AUTHENTICODE_TIMESTAMPER_INFO
        {
            public int cbsize;
            public int dwError;
            public CapiNative.AlgorithmId algHash;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftTimestamp;
            public IntPtr pChainContext;
        }

        [Flags]
        public enum AxlVerificationFlags
        {
            LifetimeSigning = 0x10,
            None = 0,
            NoRevocationCheck = 1,
            RevocationCheckEndCertOnly = 2,
            RevocationCheckEntireChain = 4,
            TrustMicrosoftRootOnly = 0x20,
            UrlOnlyCacheRetrieval = 8
        }

        [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public static class UnsafeNativeMethods
        {
            [DllImport("clr")]
            public static extern int _AxlGetIssuerPublicKeyHash(IntPtr pCertContext, out SafeAxlBufferHandle ppwszPublicKeyHash);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("clr")]
            public static extern int CertFreeAuthenticodeSignerInfo(ref X509Native.AXL_AUTHENTICODE_SIGNER_INFO pSignerInfo);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("clr")]
            public static extern int CertFreeAuthenticodeTimestamperInfo(ref X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO pTimestamperInfo);
            [DllImport("clr")]
            public static extern int CertVerifyAuthenticodeLicense(ref CapiNative.CRYPTOAPI_BLOB pLicenseBlob, X509Native.AxlVerificationFlags dwFlags, [In, Out] ref X509Native.AXL_AUTHENTICODE_SIGNER_INFO pSignerInfo, [In, Out] ref X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO pTimestamperInfo);
        }
    }
}

