namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class NativeMethods
    {
        private NativeMethods()
        {
        }

        [DllImport("KERNEL32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CloseHandle(IntPtr Handle);
        [DllImport("Kernel32", CharSet=CharSet.Auto)]
        internal static extern IntPtr GetCurrentThread();
        [DllImport("ADVAPI32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool OpenThreadToken(IntPtr ThreadHandle, uint DesiredAccess, bool OpenAsSelf, ref SafeUserTokenHandle TokenHandle);
        [DllImport("ADVAPI32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool SetThreadToken(IntPtr Thread, SafeUserTokenHandle Token);
    }
}

