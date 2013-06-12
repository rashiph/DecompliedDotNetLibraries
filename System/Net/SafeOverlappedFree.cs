namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal sealed class SafeOverlappedFree : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCloseSocket _socketHandle;
        private const int LPTR = 0x40;
        internal static readonly SafeOverlappedFree Zero = new SafeOverlappedFree(false);

        private SafeOverlappedFree() : base(true)
        {
        }

        private SafeOverlappedFree(bool ownsHandle) : base(ownsHandle)
        {
        }

        public static SafeOverlappedFree Alloc()
        {
            SafeOverlappedFree free = UnsafeNclNativeMethods.SafeNetHandlesSafeOverlappedFree.LocalAlloc(0x40, (UIntPtr) Win32.OverlappedSize);
            if (free.IsInvalid)
            {
                free.SetHandleAsInvalid();
                throw new OutOfMemoryException();
            }
            return free;
        }

        public static SafeOverlappedFree Alloc(SafeCloseSocket socketHandle)
        {
            SafeOverlappedFree free = Alloc();
            free._socketHandle = socketHandle;
            return free;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Close(bool resetOwner)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (resetOwner)
                {
                    this._socketHandle = null;
                }
                base.Close();
            }
        }

        protected override bool ReleaseHandle()
        {
            SafeCloseSocket socket = this._socketHandle;
            if ((socket != null) && !socket.IsInvalid)
            {
                socket.Dispose();
            }
            return (UnsafeNclNativeMethods.SafeNetHandles.LocalFree(base.handle) == IntPtr.Zero);
        }
    }
}

