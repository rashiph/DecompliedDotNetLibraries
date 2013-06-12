namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle() : base(true)
        {
        }

        internal SafeTokenHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return Win32Native.CloseHandle(base.handle);
        }

        internal static SafeTokenHandle InvalidHandle
        {
            get
            {
                return new SafeTokenHandle(IntPtr.Zero);
            }
        }
    }
}

