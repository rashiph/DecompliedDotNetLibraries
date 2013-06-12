namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), Obsolete("Use System.Runtime.InteropServices.ComTypes.TYPELIBATTR instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct TYPELIBATTR
    {
        public Guid guid;
        public int lcid;
        public SYSKIND syskind;
        public short wMajorVerNum;
        public short wMinorVerNum;
        public LIBFLAGS wLibFlags;
    }
}

