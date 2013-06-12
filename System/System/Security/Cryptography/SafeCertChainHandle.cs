namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class SafeCertChainHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCertChainHandle() : base(true)
        {
        }

        internal SafeCertChainHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true)]
        private static extern void CertFreeCertificateChain(IntPtr handle);
        protected override bool ReleaseHandle()
        {
            CertFreeCertificateChain(base.handle);
            return true;
        }

        internal static SafeCertChainHandle InvalidHandle
        {
            get
            {
                return new SafeCertChainHandle(IntPtr.Zero);
            }
        }
    }
}

