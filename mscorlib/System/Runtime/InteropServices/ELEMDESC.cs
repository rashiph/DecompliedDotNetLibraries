namespace System.Runtime.InteropServices
{
    using System;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), Obsolete("Use System.Runtime.InteropServices.ComTypes.ELEMDESC instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct ELEMDESC
    {
        public TYPEDESC tdesc;
        public DESCUNION desc;
        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode), ComVisible(false)]
        public struct DESCUNION
        {
            [FieldOffset(0)]
            public IDLDESC idldesc;
            [FieldOffset(0)]
            public PARAMDESC paramdesc;
        }
    }
}

