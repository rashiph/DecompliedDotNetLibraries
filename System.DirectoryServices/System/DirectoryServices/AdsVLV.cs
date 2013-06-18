namespace System.DirectoryServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class AdsVLV
    {
        public int beforeCount;
        public int afterCount;
        public int offset;
        public int contentCount;
        public IntPtr target;
        public int contextIDlength;
        public IntPtr contextID;
    }
}

