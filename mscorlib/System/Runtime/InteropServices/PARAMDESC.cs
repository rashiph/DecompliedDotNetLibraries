namespace System.Runtime.InteropServices
{
    using System;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), Obsolete("Use System.Runtime.InteropServices.ComTypes.PARAMDESC instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct PARAMDESC
    {
        public IntPtr lpVarValue;
        public PARAMFLAG wParamFlags;
    }
}

