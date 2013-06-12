namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeLocalAllocHandle : SafeBuffer
    {
        private SafeLocalAllocHandle() : base(true)
        {
        }

        internal SafeLocalAllocHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (Win32Native.LocalFree(base.handle) == IntPtr.Zero);
        }

        internal static SafeLocalAllocHandle InvalidHandle
        {
            get
            {
                return new SafeLocalAllocHandle(IntPtr.Zero);
            }
        }
    }
}

