namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeLsaReturnBufferHandle : SafeBuffer
    {
        private SafeLsaReturnBufferHandle() : base(true)
        {
        }

        internal SafeLsaReturnBufferHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (Win32Native.LsaFreeReturnBuffer(base.handle) >= 0);
        }

        internal static SafeLsaReturnBufferHandle InvalidHandle
        {
            get
            {
                return new SafeLsaReturnBufferHandle(IntPtr.Zero);
            }
        }
    }
}

