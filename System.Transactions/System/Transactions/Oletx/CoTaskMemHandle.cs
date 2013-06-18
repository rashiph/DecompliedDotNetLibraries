namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class CoTaskMemHandle : SafeHandle
    {
        public CoTaskMemHandle() : base(IntPtr.Zero, true)
        {
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr ptr);
        protected override bool ReleaseHandle()
        {
            CoTaskMemFree(base.handle);
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                if (!base.IsClosed)
                {
                    return (base.handle == IntPtr.Zero);
                }
                return true;
            }
        }
    }
}

