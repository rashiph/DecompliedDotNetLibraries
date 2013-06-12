namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StringHandleOnStack
    {
        private IntPtr m_ptr;
        internal StringHandleOnStack(IntPtr pString)
        {
            this.m_ptr = pString;
        }
    }
}

