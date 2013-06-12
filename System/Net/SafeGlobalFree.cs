namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeGlobalFree : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeGlobalFree() : base(true)
        {
        }

        private SafeGlobalFree(bool ownsHandle) : base(ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles.GlobalFree(base.handle) == IntPtr.Zero);
        }
    }
}

