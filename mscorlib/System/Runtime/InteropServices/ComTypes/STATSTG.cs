namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct STATSTG
    {
        public string pwcsName;
        public int type;
        public long cbSize;
        public System.Runtime.InteropServices.ComTypes.FILETIME mtime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ctime;
        public System.Runtime.InteropServices.ComTypes.FILETIME atime;
        public int grfMode;
        public int grfLocksSupported;
        public Guid clsid;
        public int grfStateBits;
        public int reserved;
    }
}

