namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeNativeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private bool _useLocalFree;

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeNativeMemoryHandle() : this(false)
        {
        }

        internal SafeNativeMemoryHandle(bool useLocalFree) : base(true)
        {
            this._useLocalFree = useLocalFree;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeNativeMemoryHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            if (!(base.handle != IntPtr.Zero))
            {
                return false;
            }
            if (this._useLocalFree)
            {
                Microsoft.Win32.UnsafeNativeMethods.LocalFree(base.handle);
            }
            else
            {
                Marshal.FreeHGlobal(base.handle);
            }
            base.handle = IntPtr.Zero;
            return true;
        }

        internal void SetDataHandle(IntPtr handle)
        {
            base.SetHandle(handle);
        }
    }
}

