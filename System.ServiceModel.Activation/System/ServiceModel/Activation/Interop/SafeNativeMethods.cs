namespace System.ServiceModel.Activation.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        private const string ADVAPI32 = "advapi32.dll";
        public const int ERROR_NO_TOKEN = 0x3f0;
        private const string KERNEL32 = "kernel32.dll";

        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern IntPtr GetCurrentThread();
        [SecurityCritical]
        internal static bool OpenCurrentThreadTokenCritical(TokenAccessLevels desiredAccess, bool openAsSelf, out SafeCloseHandleCritical tokenHandle, out int error)
        {
            bool flag = OpenThreadTokenCritical(GetCurrentThread(), desiredAccess, openAsSelf, out tokenHandle);
            error = Marshal.GetLastWin32Error();
            return flag;
        }

        [DllImport("advapi32.dll", EntryPoint="OpenThreadToken", SetLastError=true)]
        private static extern bool OpenThreadTokenCritical([In] IntPtr ThreadHandle, [In] TokenAccessLevels DesiredAccess, [In] bool OpenAsSelf, out SafeCloseHandleCritical TokenHandle);
    }
}

