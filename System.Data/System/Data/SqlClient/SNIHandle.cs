namespace System.Data.SqlClient
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class SNIHandle : SafeHandle
    {
        private readonly bool _fSync;
        private readonly uint _status;

        internal SNIHandle(SNINativeMethodWrapper.ConsumerInfo myInfo, string serverName, SNIHandle parent) : base(IntPtr.Zero, true)
        {
            this._status = uint.MaxValue;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this._status = SNINativeMethodWrapper.SNIOpen(myInfo, serverName, parent, ref this.handle, parent._fSync);
            }
        }

        internal SNIHandle(SNINativeMethodWrapper.ConsumerInfo myInfo, string serverName, byte[] spnBuffer, bool ignoreSniOpenTimeout, int timeout, out byte[] instanceName, bool flushCache, bool fSync, bool fParallel) : base(IntPtr.Zero, true)
        {
            this._status = uint.MaxValue;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this._fSync = fSync;
                instanceName = new byte[0x100];
                if (ignoreSniOpenTimeout)
                {
                    timeout = -1;
                }
                this._status = SNINativeMethodWrapper.SNIOpenSyncEx(myInfo, serverName, ref this.handle, spnBuffer, instanceName, flushCache, fSync, timeout, fParallel);
            }
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if ((IntPtr.Zero != handle) && (SNINativeMethodWrapper.SNIClose(handle) != 0))
            {
                return false;
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

        internal uint Status
        {
            get
            {
                return this._status;
            }
        }
    }
}

