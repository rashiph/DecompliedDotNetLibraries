namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;

    internal sealed class SqlDebugContext : IDisposable
    {
        internal bool active;
        internal byte[] data;
        internal uint dbgpid;
        internal bool fOption;
        internal IntPtr hMemMap = ADP.PtrZero;
        internal string machineName;
        internal uint pid;
        internal IntPtr pMemMap = ADP.PtrZero;
        internal string sdiDllName;
        internal uint tid;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.pMemMap != IntPtr.Zero)
            {
                NativeMethods.UnmapViewOfFile(this.pMemMap);
                this.pMemMap = IntPtr.Zero;
            }
            if (this.hMemMap != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(this.hMemMap);
                this.hMemMap = IntPtr.Zero;
            }
            this.active = false;
        }

        ~SqlDebugContext()
        {
            this.Dispose(false);
        }
    }
}

