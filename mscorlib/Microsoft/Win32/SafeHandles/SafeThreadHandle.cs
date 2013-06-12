namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeThreadHandle() : base(true)
        {
        }

        internal SafeThreadHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return Win32Native.CloseHandle(base.handle);
        }
    }
}

