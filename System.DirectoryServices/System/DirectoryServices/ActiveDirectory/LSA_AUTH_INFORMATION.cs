namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LSA_AUTH_INFORMATION
    {
        public LARGE_INTEGER LastUpdateTime;
        public int AuthType;
        public int AuthInfoLength;
        public IntPtr AuthInfo;
    }
}

