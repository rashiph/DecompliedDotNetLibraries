namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeContextBufferChannelBinding_SECUR32 : SafeFreeContextBufferChannelBinding
    {
        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles_SECUR32.FreeContextBuffer(base.handle) == 0);
        }
    }
}

