namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct TYPEATTR
    {
        public const int MEMBER_ID_NIL = -1;
        public Guid guid;
        public int lcid;
        public int dwReserved;
        public int memidConstructor;
        public int memidDestructor;
        public IntPtr lpstrSchema;
        public int cbSizeInstance;
        public System.Runtime.InteropServices.ComTypes.TYPEKIND typekind;
        public short cFuncs;
        public short cVars;
        public short cImplTypes;
        public short cbSizeVft;
        public short cbAlignment;
        public System.Runtime.InteropServices.ComTypes.TYPEFLAGS wTypeFlags;
        public short wMajorVerNum;
        public short wMinorVerNum;
        public System.Runtime.InteropServices.ComTypes.TYPEDESC tdescAlias;
        public System.Runtime.InteropServices.ComTypes.IDLDESC idldescType;
    }
}

