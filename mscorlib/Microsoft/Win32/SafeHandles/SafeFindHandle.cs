namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        internal SafeFindHandle() : base(true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return Win32Native.FindClose(base.handle);
        }
    }
}

