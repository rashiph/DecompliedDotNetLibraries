namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;
    using System.Threading;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafePerfProviderHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafePerfProviderHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            if (Interlocked.Exchange(ref this.handle, IntPtr.Zero) != IntPtr.Zero)
            {
                Microsoft.Win32.UnsafeNativeMethods.PerfStopProvider(handle);
            }
            return true;
        }
    }
}

