namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class NegotiateCallerNameRequest
    {
        public int messageType;
        public LUID logonId;
    }
}

