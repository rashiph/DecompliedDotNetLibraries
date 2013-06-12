namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct TYPELIBATTR
    {
        public Guid guid;
        public int lcid;
        public System.Runtime.InteropServices.ComTypes.SYSKIND syskind;
        public short wMajorVerNum;
        public short wMinorVerNum;
        public System.Runtime.InteropServices.ComTypes.LIBFLAGS wLibFlags;
    }
}

