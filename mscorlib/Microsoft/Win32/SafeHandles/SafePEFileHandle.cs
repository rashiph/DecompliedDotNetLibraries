namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafePEFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafePEFileHandle() : base(true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            ReleaseSafePEFileHandle(base.handle);
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void ReleaseSafePEFileHandle(IntPtr peFile);
    }
}

