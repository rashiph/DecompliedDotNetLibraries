namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class KeyContainerHandle : SafeHandle
    {
        private KeyContainerHandle() : base(IntPtr.Zero, true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("Advapi32.dll", CallingConvention=CallingConvention.StdCall)]
        private static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);
        protected override bool ReleaseHandle()
        {
            return CryptReleaseContext(base.handle, 0);
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

