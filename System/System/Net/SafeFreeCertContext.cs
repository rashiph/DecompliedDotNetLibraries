namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCertContext : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string ADVAPI32 = "advapi32.dll";
        private const uint CRYPT_ACQUIRE_SILENT_FLAG = 0x40;
        private const string CRYPT32 = "crypt32.dll";

        internal SafeFreeCertContext() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            UnsafeNclNativeMethods.SafeNetHandles.CertFreeCertificateContext(base.handle);
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Set(IntPtr value)
        {
            base.handle = value;
        }
    }
}

