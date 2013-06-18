namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class DnsSrvData
    {
        public string targetName;
        public short priority;
        public short weight;
        public short port;
        public short pad;
    }
}

