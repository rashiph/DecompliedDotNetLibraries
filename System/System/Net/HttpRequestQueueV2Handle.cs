namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class HttpRequestQueueV2Handle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        private int disposed;

        private HttpRequestQueueV2Handle()
        {
        }

        internal IntPtr DangerousGetHandle()
        {
            return base.handle;
        }

        protected override bool ReleaseHandle()
        {
            if (!this.IsInvalid && (Interlocked.Increment(ref this.disposed) == 1))
            {
                return (UnsafeNclNativeMethods.SafeNetHandles.HttpCloseRequestQueue(base.handle) == 0);
            }
            return true;
        }
    }
}

