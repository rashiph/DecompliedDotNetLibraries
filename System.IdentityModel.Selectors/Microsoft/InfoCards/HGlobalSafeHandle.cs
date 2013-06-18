namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class HGlobalSafeHandle : SafeHandle
    {
        private int m_bytes;

        private HGlobalSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private HGlobalSafeHandle(IntPtr toManage, int length) : base(IntPtr.Zero, true)
        {
            this.m_bytes = length;
            base.SetHandle(toManage);
        }

        public static HGlobalSafeHandle Construct()
        {
            return new HGlobalSafeHandle();
        }

        public static HGlobalSafeHandle Construct(int bytes)
        {
            return new HGlobalSafeHandle(Marshal.AllocHGlobal(bytes), bytes);
        }

        public static HGlobalSafeHandle Construct(string managedString)
        {
            return new HGlobalSafeHandle(Marshal.StringToHGlobalUni(managedString), (managedString.Length + 1) * 2);
        }

        protected override bool ReleaseHandle()
        {
            ZeroMemory(base.handle, this.m_bytes);
            Marshal.FreeHGlobal(base.handle);
            return true;
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", EntryPoint="RtlZeroMemory")]
        public static extern void ZeroMemory(IntPtr dest, int size);

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }
    }
}

