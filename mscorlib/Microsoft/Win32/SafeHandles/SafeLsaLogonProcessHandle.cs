namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeLsaLogonProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeLsaLogonProcessHandle() : base(true)
        {
        }

        internal SafeLsaLogonProcessHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (Win32Native.LsaDeregisterLogonProcess(base.handle) >= 0);
        }

        internal static SafeLsaLogonProcessHandle InvalidHandle
        {
            get
            {
                return new SafeLsaLogonProcessHandle(IntPtr.Zero);
            }
        }
    }
}

