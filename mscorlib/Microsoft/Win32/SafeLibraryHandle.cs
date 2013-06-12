namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeLibraryHandle() : base(true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.FreeLibrary(base.handle);
        }
    }
}

