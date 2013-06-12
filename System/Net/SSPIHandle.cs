namespace System.Net
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct SSPIHandle
    {
        private IntPtr HandleHi;
        private IntPtr HandleLo;
        public bool IsZero
        {
            get
            {
                return ((this.HandleHi == IntPtr.Zero) && (this.HandleLo == IntPtr.Zero));
            }
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void SetToInvalid()
        {
            this.HandleHi = IntPtr.Zero;
            this.HandleLo = IntPtr.Zero;
        }

        public override string ToString()
        {
            return (this.HandleHi.ToString("x") + ":" + this.HandleLo.ToString("x"));
        }
    }
}

