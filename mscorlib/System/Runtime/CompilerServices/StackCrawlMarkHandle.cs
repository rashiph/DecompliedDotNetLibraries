namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StackCrawlMarkHandle
    {
        private IntPtr m_ptr;
        internal StackCrawlMarkHandle(IntPtr stackMark)
        {
            this.m_ptr = stackMark;
        }
    }
}

