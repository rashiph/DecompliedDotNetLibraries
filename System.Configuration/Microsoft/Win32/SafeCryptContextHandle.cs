namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;
    using System.Security.Permissions;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeCryptContextHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeCryptContextHandle() : base(true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeCryptContextHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            if (base.handle != IntPtr.Zero)
            {
                Microsoft.Win32.UnsafeNativeMethods.CryptReleaseContext(this, 0);
                return true;
            }
            return false;
        }
    }
}

