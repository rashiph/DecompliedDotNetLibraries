namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    internal struct MULTI_QI : IDisposable
    {
        internal IntPtr piid;
        internal IntPtr pItf;
        internal int hr;
        internal MULTI_QI(IntPtr pid)
        {
            this.piid = pid;
            this.pItf = IntPtr.Zero;
            this.hr = 0;
        }

        void IDisposable.Dispose()
        {
            if (this.pItf != IntPtr.Zero)
            {
                Marshal.Release(this.pItf);
                this.pItf = IntPtr.Zero;
            }
            if (this.piid != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.piid);
                this.piid = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
    }
}

