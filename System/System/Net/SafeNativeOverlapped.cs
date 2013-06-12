namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class SafeNativeOverlapped : SafeHandle
    {
        internal static readonly SafeNativeOverlapped Zero = new SafeNativeOverlapped();

        internal SafeNativeOverlapped() : this(IntPtr.Zero)
        {
        }

        internal unsafe SafeNativeOverlapped(NativeOverlapped* handle) : this((IntPtr) handle)
        {
        }

        internal SafeNativeOverlapped(IntPtr handle) : base(IntPtr.Zero, true)
        {
            base.SetHandle(handle);
        }

        protected override unsafe bool ReleaseHandle()
        {
            IntPtr ptr = Interlocked.Exchange(ref this.handle, IntPtr.Zero);
            if ((ptr != IntPtr.Zero) && !NclUtilities.HasShutdownStarted)
            {
                Overlapped.Free((NativeOverlapped*) ptr);
            }
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                return (base.handle == IntPtr.Zero);
            }
        }
    }
}

