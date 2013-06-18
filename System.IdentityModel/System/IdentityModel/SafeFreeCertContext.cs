namespace System.IdentityModel
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class SafeFreeCertContext : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string ADVAPI32 = "advapi32.dll";
        private const uint CRYPT_ACQUIRE_SILENT_FLAG = 0x40;
        private const string CRYPT32 = "crypt32.dll";

        internal SafeFreeCertContext() : base(true)
        {
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true, ExactSpelling=true)]
        private static extern bool CertFreeCertificateContext([In] IntPtr certContext);
        protected override bool ReleaseHandle()
        {
            return CertFreeCertificateContext(base.handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Set(IntPtr value)
        {
            base.handle = value;
        }
    }
}

