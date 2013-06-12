namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct VARDESC
    {
        public int memid;
        public string lpstrSchema;
        public DESCUNION desc;
        public System.Runtime.InteropServices.ComTypes.ELEMDESC elemdescVar;
        public short wVarFlags;
        public VARKIND varkind;
        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
        public struct DESCUNION
        {
            [FieldOffset(0)]
            public IntPtr lpvarValue;
            [FieldOffset(0)]
            public int oInst;
        }
    }
}

