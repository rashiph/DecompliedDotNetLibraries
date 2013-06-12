namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeContextBuffer_SCHANNEL : SafeFreeContextBuffer
    {
        private const string SCHANNEL = "schannel.dll";

        internal SafeFreeContextBuffer_SCHANNEL()
        {
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.FreeContextBuffer(base.handle) == 0);
        }
    }
}

