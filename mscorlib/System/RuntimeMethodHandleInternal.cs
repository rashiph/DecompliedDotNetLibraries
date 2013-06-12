namespace System
{
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RuntimeMethodHandleInternal
    {
        internal IntPtr m_handle;
        internal static RuntimeMethodHandleInternal EmptyHandle
        {
            get
            {
                return new RuntimeMethodHandleInternal();
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
        internal RuntimeMethodHandleInternal(IntPtr value)
        {
            this.m_handle = value;
        }
    }
}

