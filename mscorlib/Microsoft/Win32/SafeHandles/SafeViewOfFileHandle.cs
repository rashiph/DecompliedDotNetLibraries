namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        internal SafeViewOfFileHandle() : base(true)
        {
        }

        [SecurityCritical]
        internal SafeViewOfFileHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            if (Win32Native.UnmapViewOfFile(base.handle))
            {
                base.handle = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }
}

