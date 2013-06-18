namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal sealed class KeyHandle : SafeHandle
    {
        private KeyHandle() : base(IntPtr.Zero, true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("Advapi32.dll", CallingConvention=CallingConvention.StdCall)]
        private static extern bool CryptDestroyKey(IntPtr hKey);
        protected override bool ReleaseHandle()
        {
            return CryptDestroyKey(base.handle);
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

