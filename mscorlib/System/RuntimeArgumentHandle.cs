namespace System
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct RuntimeArgumentHandle
    {
        private IntPtr m_ptr;
        internal IntPtr Value
        {
            get
            {
                return this.m_ptr;
            }
        }
    }
}

