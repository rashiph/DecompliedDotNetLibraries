namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeCertChainHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCertChainHandle() : base(true)
        {
        }

        internal SafeCertChainHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("crypt32.dll", SetLastError=true)]
        private static extern void CertFreeCertificateChain(IntPtr handle);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            CertFreeCertificateChain(base.handle);
            return true;
        }

        internal static System.Security.Cryptography.SafeCertChainHandle InvalidHandle
        {
            get
            {
                return new System.Security.Cryptography.SafeCertChainHandle(IntPtr.Zero);
            }
        }
    }
}

