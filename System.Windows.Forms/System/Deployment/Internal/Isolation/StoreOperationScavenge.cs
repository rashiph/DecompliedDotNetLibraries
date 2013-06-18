namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationScavenge
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.U8)]
        public ulong SizeReclaimationLimit;
        [MarshalAs(UnmanagedType.U8)]
        public ulong RuntimeLimit;
        [MarshalAs(UnmanagedType.U4)]
        public uint ComponentCountLimit;
        [SecuritySafeCritical]
        public StoreOperationScavenge(bool Light, ulong SizeLimit, ulong RunLimit, uint ComponentLimit)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationScavenge));
            this.Flags = OpFlags.Nothing;
            if (Light)
            {
                this.Flags |= OpFlags.Light;
            }
            this.SizeReclaimationLimit = SizeLimit;
            if (SizeLimit != 0L)
            {
                this.Flags |= OpFlags.LimitSize;
            }
            this.RuntimeLimit = RunLimit;
            if (RunLimit != 0L)
            {
                this.Flags |= OpFlags.LimitTime;
            }
            this.ComponentCountLimit = ComponentLimit;
            if (ComponentLimit != 0)
            {
                this.Flags |= OpFlags.LimitCount;
            }
        }

        public StoreOperationScavenge(bool Light) : this(Light, 0L, 0L, 0)
        {
        }

        public void Destroy()
        {
        }
        [Flags]
        public enum OpFlags
        {
            Light = 1,
            LimitCount = 8,
            LimitSize = 2,
            LimitTime = 4,
            Nothing = 0
        }
    }
}

