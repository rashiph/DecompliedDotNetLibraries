namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        internal SafeFileMappingHandle() : base(true)
        {
        }

        [SecurityCritical]
        internal SafeFileMappingHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
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

