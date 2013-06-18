namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    internal struct MULTI_QI_X64 : IDisposable
    {
        internal IntPtr piid;
        internal IntPtr pItf;
        internal int hr;
        internal int padding;
        internal MULTI_QI_X64(IntPtr pid)
        {
            this.piid = pid;
            this.pItf = IntPtr.Zero;
            this.hr = 0;
            this.padding = 0;
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

