namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal class SafeCompressedStackHandle : SafeHandle
    {
        public SafeCompressedStackHandle() : base(IntPtr.Zero, true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            CompressedStack.DestroyDelayedCompressedStack(base.handle);
            base.handle = IntPtr.Zero;
            return true;
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get
            {
                return (base.handle == IntPtr.Zero);
            }
        }
    }
}

