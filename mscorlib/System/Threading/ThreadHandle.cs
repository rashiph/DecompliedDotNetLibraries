namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ThreadHandle
    {
        private IntPtr m_ptr;
        internal ThreadHandle(IntPtr pThread)
        {
            this.m_ptr = pThread;
        }
    }
}

