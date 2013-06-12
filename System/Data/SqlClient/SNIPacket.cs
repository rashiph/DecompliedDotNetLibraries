namespace System.Data.SqlClient
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class SNIPacket : SafeHandle
    {
        internal SNIPacket(SafeHandle sniHandle) : base(IntPtr.Zero, true)
        {
            SNINativeMethodWrapper.SNIPacketAllocate(sniHandle, SNINativeMethodWrapper.IOType.WRITE, ref this.handle);
            if (IntPtr.Zero == base.handle)
            {
                throw SQL.SNIPacketAllocationFailure();
            }
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                SNINativeMethodWrapper.SNIPacketRelease(handle);
            }
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }
    }
}

