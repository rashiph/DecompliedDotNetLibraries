namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeCloseHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        private int _disposed;
        private const string ADVAPI32 = "advapi32.dll";
        private const string HTTPAPI = "httpapi.dll";
        private const string SECURITY = "security.dll";

        private SafeCloseHandle()
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Abort()
        {
            this.ReleaseHandle();
            base.SetHandleAsInvalid();
        }

        internal IntPtr DangerousGetHandle()
        {
            return base.handle;
        }

        protected override bool ReleaseHandle()
        {
            if (!this.IsInvalid && (Interlocked.Increment(ref this._disposed) == 1))
            {
                return UnsafeNclNativeMethods.SafeNetHandles.CloseHandle(base.handle);
            }
            return true;
        }
    }
}

