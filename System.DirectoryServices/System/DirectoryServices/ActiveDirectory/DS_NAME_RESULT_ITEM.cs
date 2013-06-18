namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_NAME_RESULT_ITEM
    {
        public DS_NAME_ERROR status;
        public IntPtr pDomain;
        public IntPtr pName;
    }
}

