namespace System
{
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RuntimeFieldHandleInternal
    {
        internal IntPtr m_handle;
        internal static RuntimeFieldHandleInternal EmptyHandle
        {
            get
            {
                return new RuntimeFieldHandleInternal();
            }
        }
        internal bool IsNullHandle()
        {
            return this.m_handle.IsNull();
        }

        internal IntPtr Value
        {
            [SecurityCritical]
            get
            {
                return this.m_handle;
            }
        }
        [SecurityCritical]
        internal RuntimeFieldHandleInternal(IntPtr value)
        {
            this.m_handle = value;
        }
    }
}

