namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeContextBuffer_SECURITY : SafeFreeContextBuffer
    {
        private const string SECURITY = "security.dll";

        internal SafeFreeContextBuffer_SECURITY()
        {
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles_SECURITY.FreeContextBuffer(base.handle) == 0);
        }
    }
}

