namespace System.Transactions
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class SafeIUnknown : SafeHandle
    {
        internal SafeIUnknown() : base(IntPtr.Zero, true)
        {
        }

        internal SafeIUnknown(IntPtr unknown) : base(IntPtr.Zero, true)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                base.handle = unknown;
            }
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                Marshal.Release(handle);
            }
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                if (!base.IsClosed)
                {
                    return (IntPtr.Zero == base.handle);
                }
                return true;
            }
        }
    }
}

