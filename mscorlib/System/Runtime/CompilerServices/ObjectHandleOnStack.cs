namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjectHandleOnStack
    {
        private IntPtr m_ptr;
        internal ObjectHandleOnStack(IntPtr pObject)
        {
            this.m_ptr = pObject;
        }
    }
}

