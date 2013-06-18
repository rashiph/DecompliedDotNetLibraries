namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal class CertificateHandle : SafeHandle
    {
        protected bool delete;

        protected CertificateHandle() : base(IntPtr.Zero, true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        private static extern bool CertDeleteCertificateFromStore(IntPtr pCertContext);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall)]
        private static extern bool CertFreeCertificateContext(IntPtr pCertContext);
        protected override bool ReleaseHandle()
        {
            if (this.delete)
            {
                return CertDeleteCertificateFromStore(base.handle);
            }
            return CertFreeCertificateContext(base.handle);
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

