namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class CertificateStoreHandle : SafeHandle
    {
        private CertificateStoreHandle() : base(IntPtr.Zero, true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall)]
        private static extern bool CertCloseStore(IntPtr hCertStore, int dwFlags);
        protected override bool ReleaseHandle()
        {
            return CertCloseStore(base.handle, 0);
        }

        public override bool IsInvalid
        {
            get
            {
                return (base.handle == IntPtr.Zero);
            }
        }
    }
}

