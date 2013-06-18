namespace System.IdentityModel.Selectors
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
    internal class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeLibraryHandle() : base(true)
        {
        }

        [DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern System.IdentityModel.Selectors.SafeLibraryHandle LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string dllname);
        protected override bool ReleaseHandle()
        {
            return FreeLibrary(base.handle);
        }
    }
}

