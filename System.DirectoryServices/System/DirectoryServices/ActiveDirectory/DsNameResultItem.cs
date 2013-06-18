namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class DsNameResultItem
    {
        public int status;
        public string domain;
        public string name;
    }
}

