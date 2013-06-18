namespace System.ServiceModel.Channels
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeViewOfFileHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (UnsafeNativeMethods.UnmapViewOfFile(base.handle) != 0)
            {
                base.handle = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }
}

