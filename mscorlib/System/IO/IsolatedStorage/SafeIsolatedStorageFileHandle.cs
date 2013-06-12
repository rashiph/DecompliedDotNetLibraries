namespace System.IO.IsolatedStorage
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeIsolatedStorageFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeIsolatedStorageFileHandle() : base(true)
        {
            base.SetHandle(IntPtr.Zero);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void Close(IntPtr file);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            Close(base.handle);
            return true;
        }
    }
}

