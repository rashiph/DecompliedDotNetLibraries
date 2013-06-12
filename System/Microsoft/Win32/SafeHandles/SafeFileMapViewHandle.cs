namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal sealed class SafeFileMapViewHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFileMapViewHandle() : base(true)
        {
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern SafeFileMapViewHandle MapViewOfFile(Microsoft.Win32.SafeHandles.SafeFileMappingHandle hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);
        protected override bool ReleaseHandle()
        {
            return UnmapViewOfFile(base.handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        private static extern bool UnmapViewOfFile(IntPtr handle);
    }
}

