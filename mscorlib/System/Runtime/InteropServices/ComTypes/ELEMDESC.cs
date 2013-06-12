namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct ELEMDESC
    {
        public System.Runtime.InteropServices.ComTypes.TYPEDESC tdesc;
        public DESCUNION desc;
        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
        public struct DESCUNION
        {
            [FieldOffset(0)]
            public System.Runtime.InteropServices.ComTypes.IDLDESC idldesc;
            [FieldOffset(0)]
            public System.Runtime.InteropServices.ComTypes.PARAMDESC paramdesc;
        }
    }
}

