namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeProcessHandle() : base(true)
        {
        }

        internal SafeProcessHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return Win32Native.CloseHandle(base.handle);
        }

        internal static SafeProcessHandle InvalidHandle
        {
            get
            {
                return new SafeProcessHandle(IntPtr.Zero);
            }
        }
    }
}

