namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeLsaMemoryHandle : SafeBuffer
    {
        private SafeLsaMemoryHandle() : base(true)
        {
        }

        internal SafeLsaMemoryHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (Win32Native.LsaFreeMemory(base.handle) == 0);
        }

        internal static SafeLsaMemoryHandle InvalidHandle
        {
            get
            {
                return new SafeLsaMemoryHandle(IntPtr.Zero);
            }
        }
    }
}

