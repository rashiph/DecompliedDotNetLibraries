namespace System.ServiceModel.Channels
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private bool doNotfreeLibraryOnRelease;

        internal SafeLibraryHandle() : base(true)
        {
            this.doNotfreeLibraryOnRelease = false;
        }

        public void DoNotFreeLibraryOnRelease()
        {
            this.doNotfreeLibraryOnRelease = true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        private static extern bool FreeLibrary(IntPtr hModule);
        protected override bool ReleaseHandle()
        {
            if (this.doNotfreeLibraryOnRelease)
            {
                base.handle = IntPtr.Zero;
                return true;
            }
            return FreeLibrary(base.handle);
        }
    }
}

