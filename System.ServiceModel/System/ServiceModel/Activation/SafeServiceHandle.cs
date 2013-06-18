namespace System.ServiceModel.Activation
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeServiceHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return ListenerUnsafeNativeMethods.CloseServiceHandle(base.handle);
        }
    }
}

