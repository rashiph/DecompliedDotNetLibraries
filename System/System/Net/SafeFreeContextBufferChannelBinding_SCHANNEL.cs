namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeContextBufferChannelBinding_SCHANNEL : SafeFreeContextBufferChannelBinding
    {
        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.FreeContextBuffer(base.handle) == 0);
        }
    }
}

