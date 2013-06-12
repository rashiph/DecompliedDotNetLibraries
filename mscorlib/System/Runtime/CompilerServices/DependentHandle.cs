namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    internal struct DependentHandle
    {
        private IntPtr _handle;
        [SecurityCritical]
        public DependentHandle(object primary, object secondary)
        {
            IntPtr zero = IntPtr.Zero;
            nInitialize(primary, secondary, out zero);
            this._handle = zero;
        }

        public bool IsAllocated
        {
            get
            {
                return (this._handle != IntPtr.Zero);
            }
        }
        [SecurityCritical]
        public object GetPrimary()
        {
            object obj2;
            nGetPrimary(this._handle, out obj2);
            return obj2;
        }

        [SecurityCritical]
        public void GetPrimaryAndSecondary(out object primary, out object secondary)
        {
            nGetPrimaryAndSecondary(this._handle, out primary, out secondary);
        }

        [SecurityCritical]
        public void Free()
        {
            if (this._handle != IntPtr.Zero)
            {
                IntPtr dependentHandle = this._handle;
                this._handle = IntPtr.Zero;
                nFree(dependentHandle);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nInitialize(object primary, object secondary, out IntPtr dependentHandle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nGetPrimary(IntPtr dependentHandle, out object primary);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nGetPrimaryAndSecondary(IntPtr dependentHandle, out object primary, out object secondary);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nFree(IntPtr dependentHandle);
    }
}

