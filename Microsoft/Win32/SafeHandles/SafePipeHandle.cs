namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical(SecurityCriticalScope.Everything), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
    public sealed class SafePipeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafePipeHandle() : base(true)
        {
        }

        public SafePipeHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(preexistingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return Microsoft.Win32.UnsafeNativeMethods.CloseHandle(base.handle);
        }
    }
}

