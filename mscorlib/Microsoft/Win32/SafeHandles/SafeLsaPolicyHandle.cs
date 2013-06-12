namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeLsaPolicyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeLsaPolicyHandle() : base(true)
        {
        }

        internal SafeLsaPolicyHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (Win32Native.LsaClose(base.handle) == 0);
        }

        internal static SafeLsaPolicyHandle InvalidHandle
        {
            get
            {
                return new SafeLsaPolicyHandle(IntPtr.Zero);
            }
        }
    }
}

