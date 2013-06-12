namespace System.Runtime.InteropServices
{
    using System;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), Obsolete("Use System.Runtime.InteropServices.ComTypes.VARDESC instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct VARDESC
    {
        public int memid;
        public string lpstrSchema;
        public ELEMDESC elemdescVar;
        public short wVarFlags;
        public VarEnum varkind;
        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode), ComVisible(false)]
        public struct DESCUNION
        {
            [FieldOffset(0)]
            public IntPtr lpvarValue;
            [FieldOffset(0)]
            public int oInst;
        }
    }
}

