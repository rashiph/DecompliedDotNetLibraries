namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    public sealed class SafeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeFileHandle() : base(true)
        {
        }

        public SafeFileHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(preexistingHandle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return Win32Native.CloseHandle(base.handle);
        }
    }
}

