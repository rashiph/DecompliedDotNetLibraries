namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal static Microsoft.Win32.SafeHandles.SafeProcessHandle InvalidHandle = new Microsoft.Win32.SafeHandles.SafeProcessHandle(IntPtr.Zero);

        internal SafeProcessHandle() : base(true)
        {
        }

        internal SafeProcessHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        internal void InitialSetHandle(IntPtr h)
        {
            base.handle = h;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcess(int access, bool inherit, int processId);
        protected override bool ReleaseHandle()
        {
            return Microsoft.Win32.SafeNativeMethods.CloseHandle(base.handle);
        }
    }
}

