namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeCertStoreHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCertStoreHandle() : base(true)
        {
        }

        internal SafeCertStoreHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true)]
        private static extern bool CertCloseStore(IntPtr hCertStore, uint dwFlags);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return CertCloseStore(base.handle, 0);
        }

        internal static System.Security.Cryptography.SafeCertStoreHandle InvalidHandle
        {
            get
            {
                return new System.Security.Cryptography.SafeCertStoreHandle(IntPtr.Zero);
            }
        }
    }
}

