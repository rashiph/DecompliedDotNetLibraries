namespace System.EnterpriseServices.Internal
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal sealed class SafeUserTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeUserTokenHandle() : base(true)
        {
        }

        internal SafeUserTokenHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(existingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(base.handle);
        }
    }
}

