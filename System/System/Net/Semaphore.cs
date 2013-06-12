namespace System.Net
{
    using System;
    using System.Threading;

    internal sealed class Semaphore : WaitHandle
    {
        internal Semaphore(int initialCount, int maxCount)
        {
            lock (this)
            {
                this.Handle = UnsafeNclNativeMethods.CreateSemaphore(IntPtr.Zero, initialCount, maxCount, IntPtr.Zero);
            }
        }

        internal bool ReleaseSemaphore()
        {
            return UnsafeNclNativeMethods.ReleaseSemaphore(this.Handle, 1, IntPtr.Zero);
        }
    }
}

