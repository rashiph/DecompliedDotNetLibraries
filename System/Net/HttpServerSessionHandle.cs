namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class HttpServerSessionHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        private int disposed;
        private ulong serverSessionId;

        internal HttpServerSessionHandle(ulong id)
        {
            this.serverSessionId = id;
            base.SetHandle(new IntPtr(1));
        }

        internal ulong DangerousGetServerSessionId()
        {
            return this.serverSessionId;
        }

        protected override bool ReleaseHandle()
        {
            if (!this.IsInvalid && (Interlocked.Increment(ref this.disposed) == 1))
            {
                return (UnsafeNclNativeMethods.HttpApi.HttpCloseServerSession(this.serverSessionId) == 0);
            }
            return true;
        }
    }
}

