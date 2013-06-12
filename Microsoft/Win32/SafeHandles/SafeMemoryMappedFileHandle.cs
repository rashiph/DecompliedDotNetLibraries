namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    public sealed class SafeMemoryMappedFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeMemoryMappedFileHandle() : base(true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeMemoryMappedFileHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return Microsoft.Win32.UnsafeNativeMethods.CloseHandle(base.handle);
        }
    }
}

