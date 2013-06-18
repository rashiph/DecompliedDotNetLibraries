namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class SECURITY_ATTRIBUTES
    {
        internal int nLength;
        internal IntPtr lpSecurityDescriptor = IntPtr.Zero;
        internal int bInheritHandle;
    }
}

