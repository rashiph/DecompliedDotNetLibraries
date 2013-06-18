namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr GetProcAddress(SafeLibraryHandle hModule, [MarshalAs(UnmanagedType.LPStr)] string procname);
        public static IntPtr GetProcAddressWrapper(SafeLibraryHandle implDll, string procName)
        {
            IntPtr procAddress = GetProcAddress(implDll, procName);
            if (IntPtr.Zero == procAddress)
            {
                ThrowWin32ExceptionWithContext(new Win32Exception(), procName);
            }
            return procAddress;
        }

        public static Win32Exception ThrowWin32ExceptionWithContext(Win32Exception wex, string context)
        {
            throw InfoCardTrace.ThrowHelperError(new Win32Exception(wex.NativeErrorCode, Microsoft.InfoCards.SR.GetString("ClientAPIDetailedExceptionMessage", new object[] { wex.Message, context })));
        }
    }
}

