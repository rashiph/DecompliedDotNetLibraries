namespace Microsoft.Build.Tasks.InteropUtilities
{
    using System;
    using System.Runtime.InteropServices;

    internal class RCWForCurrentContext<T> : IDisposable where T: class
    {
        private T rcwForCurrentCtx;
        private bool shouldReleaseRCW;

        public RCWForCurrentContext(T rcw)
        {
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(rcw);
            object objectForIUnknown = null;
            try
            {
                objectForIUnknown = Marshal.GetObjectForIUnknown(iUnknownForObject);
            }
            finally
            {
                Marshal.Release(iUnknownForObject);
            }
            if (objectForIUnknown == null)
            {
                this.shouldReleaseRCW = false;
                this.rcwForCurrentCtx = rcw;
            }
            else
            {
                this.shouldReleaseRCW = true;
                this.rcwForCurrentCtx = objectForIUnknown as T;
            }
        }

        private void CleanupComObject()
        {
            try
            {
                if (((this.rcwForCurrentCtx != null) && this.shouldReleaseRCW) && Marshal.IsComObject(this.rcwForCurrentCtx))
                {
                    Marshal.ReleaseComObject(this.rcwForCurrentCtx);
                }
            }
            finally
            {
                this.rcwForCurrentCtx = default(T);
            }
        }

        public void Dispose()
        {
            this.CleanupComObject();
            GC.SuppressFinalize(this);
        }

        ~RCWForCurrentContext()
        {
            this.CleanupComObject();
        }

        public T RCW
        {
            get
            {
                if (this.rcwForCurrentCtx == null)
                {
                    throw new ObjectDisposedException("RCWForCurrentCtx");
                }
                return this.rcwForCurrentCtx;
            }
        }
    }
}

